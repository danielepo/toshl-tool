module ToshClient
open MovimentiModelBuilder
open ToshlTypes
open Newtonsoft.Json
open RestSharp
open System
open System.IO

let private getByLink link deserializer =

    let rec gew page (link:string) = 
        let resource = 
            let pageAttribute = if link.Contains("?") then sprintf "&page=%d" page else sprintf "?page=%d" page 
            sprintf "%s%s" link pageAttribute
    
        Request.get resource Method.GET
        |> Request.excecute 
        |> function
            | Request.PagedContent (p,c) -> deserializer c :: gew p link
            | Request.Content c -> deserializer c :: []
            | _  -> []

    gew 0 link

module Serializer =
    let private goSerial = true
    let internal serializeToFile file obj = 
        if goSerial then
            let path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + file
            let serial = JsonConvert.SerializeObject (obj)
            System.IO.File.AppendAllText(path,serial)
        else
            ()

    let internal tryGetFile file =
        if goSerial then
            let path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + file
            match File.Exists(path) with
            | true -> Some (System.IO.File.ReadAllText(path))
            | false -> None
        else
            None
    
let deserilizer<'T> x =
    JsonConvert.DeserializeObject<'T>(x)

module Entities =

    let loadEntity entity fileName (deserializer:string -> 'a list) = 

        match Serializer.tryGetFile fileName with
        | Some file -> deserializer file
        | None ->
            let result = getByLink entity deserializer |> List.concat
            Serializer.serializeToFile fileName result
            result

    let getTags() = 
        loadEntity "/tags" "tags.json" deserilizer<Tag list>

    let getCategories() = 
        loadEntity  "/categories" "categories.json" deserilizer<Category list>
    

    let getAccounts() = 
        loadEntity "/accounts" "accounts.json" deserilizer<Account list>


let getEntries (f:DateTime) (t:DateTime) = 
    let entriesFile = "entries.json"

    match Serializer.tryGetFile entriesFile with
    | Some file -> deserilizer<Entry list> file
    | None ->
        let link = sprintf "/entries?from=%s&to=%s" (f.ToString("yyyy-MM-dd")) (t.ToString("yyyy-MM-dd"))
        let entries = 
            getByLink link deserilizer<Entry list> 
            |> List.concat 
            |> List.sortBy (fun x -> x.date)

        Serializer.serializeToFile entriesFile entries
        entries


let GetEntriesByTag (f:DateTime) (t:DateTime) = 
    let tagGroupToTaggedEntries (tags:string list,entry:Entry seq) =
        let allTags = Entities.getTags()
        let findTag tag = allTags |> List.find(fun x -> x.id = tag) 
        
        if  obj.ReferenceEquals (tags, null)then  [] else tags
        |> List.map (fun tag -> {Key = findTag tag; Value =  entry})
        |> Seq.ofList

    (getEntries f t )
    |> Seq.ofList 
    |> Seq.groupBy (fun x -> x.tags)
    |> Seq.map tagGroupToTaggedEntries
    |> Seq.concat 

let SaveRecords account path file= 
    
    let tags = Entities.getTags()
    let getTag (x : ReportVm) = tags |> List.filter (fun t -> t.id = x.Tag.ToString())
    let categories = Entities.getCategories()
    
    let getCategory (x : ReportVm) = 
        let tag = getTag x |> List.head
        if not categories.IsEmpty then 
            (categories
             |> List.filter (fun t -> t.id = tag.category)
             |> List.head).id
        else ""
    
    let createEntry (x : ReportVm) = 
        { amount = 
              if x.Type = CatType.Expence then -x.Ammount
              else x.Ammount
          currency = { code = "EUR" }
          date = x.Date.ToString("yyyy-MM-dd")
          desc = x.Description.Replace("R.I.D.                             CRED. ", "")
          account = account
          category = getCategory x
          tags = getTag x |> List.map (fun t -> t.id)
          completed = true }
    
    let entries =
        Movimenti path file
        |> Seq.filter (fun x -> x.Tagged)
        |> Seq.map createEntry

    let setEntry (entry : Entry) = 
    
        Request.get "/entries" Method.POST
        |> Request.setFormat DataFormat.Json
        |> Request.setHeader "Content-Type" "application/json"
        |> Request.setBody entry
        |> Request.excecute

    for entry in entries do
        let record:DataAccessLayer.Record = {
            Ammount = entry.amount
            Category = entry.category 
                |> System.Int32.TryParse 
                |> fun (b,x)-> if b then x else 0
            Tag= entry.tags 
                |> List.map (fun x -> System.Int32.TryParse x)
                |> List.filter (fun (b,_) -> b)
                |> List.map (fun (_,x) -> x)
            Date = entry.date
                |> System.DateTime.TryParse 
                |> fun (b,x)-> if b then x else (new DateTime())
            Description = entry.desc
        }

        DataAccessLayer.MovementSaver.insertAMovement record 
//        setEntry  entry |> ignore
    
  
