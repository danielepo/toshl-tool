module ToshClient

open Newtonsoft.Json
open Parser
open RestSharp
open System
open System.Net
open System.Linq

type TagCount = 
    { entries : int
      unsorted_entries : int
      budgets : int }

type Tag = 
    { id : string
      name : string
      name_override : bool
      modified : DateTime
      ``type`` : string
      category : string
      count : TagCount
      deleted : bool
      transient_created : DateTime
      transient_valid_till : string
      meta_tag : bool
      extra : obj }

type CategoryCount = 
    { entries : int
      income_entries : int
      expense_entries : int
      tags_used_with_category : int
      income_tags_used_with_category : int
      expense_tags_used_with_category : int
      tags : int
      income_tags : int
      expense_tags : int
      budgets : int }

type CategoryType = 
    | Expence
    | Income
    | System

type Category = 
    { id : string
      name : string
      modified : DateTime
      ``type`` : string
      deleted : bool
      name_override : bool
      transient_created : DateTime
      transient_valid_till : string
      counts : CategoryCount
      extra : obj }

type Currency = 
    { code : string }

type AccountMedian = 
    { expenses : double
      incomes : double }

type AccountGoal = 
    { amount : double
      start : string
      ``end`` : string }

type Account = 
    { id : string
      name : string
      parent : string
      name_override : string
      balance : double
      initial_balance : double
      currency : Currency
      daily_sum_median : AccountMedian
      status : string
      order : int
      modified : DateTime
      goal : AccountGoal
      deleted : bool
      transient_created : string
      transient_valid_till : string
      extra : obj }

type Location = 
    { id : string
      latitude : double
      longitude : double }

type Repeat = 
    { id : string
      start : DateTime
      ``end`` : DateTime
      frequency : string
      interval : int
      count : int
      byday : string
      bymonthday : string
      bysetps : string
      iteration : double }

type Transaction = 
    { id : string
      account : string
      currency : Currency }

type Image = 
    { id : string
      path : string
      status : string }

type Reminder = 
    { period : string
      number : int
      at : string }

type Entry = 
    { amount : double
      currency : Currency
      date : string
      desc : string
      account : string
      category : string
      tags : string list
      //    location: Location;
      //    repeat: Repeat;
      //    transaction: Transaction;
      //    images: Image list;
      //    reminders: Reminder list;
      completed : bool }

let client = new RestClient("https://api.toshl.com")
let getRequest (resource : string) (``method`` : Method) = RestRequest(resource, ``method``)


let getConfiguration (key:string) =
//    System.Configuration.ConfigurationManager.AppSettings.Get(key)
    match key with
    | "proxy_addr" -> "http://proxyfull.servizi.ras:80"
    | "proxy_user" -> "eul0856"
    | "proxy_pass" -> "Dony2207!"
    | "auth_token" -> "Basic ZWYyZDAyMDktZjRiMC00NjAxLTk1NTQtOWY5MzI1NGFhNjlmNWFlYThhMjUtMWM4ZS00ZTcyLTk5NzUtNTFmMjQzNjlhNGYzOg=="

let existsConfiguration (key:string) =
    getConfiguration key <> null

//    extra: obj;
let addAuthorization (request : IRestRequest) = 
    request.AddHeader
        ("Authorization", getConfiguration "auth_token")

let init() = //()
    let useProxy = ["proxy_addr";"proxy_user";"proxy_pass"] |> List.map existsConfiguration |> List.fold (&&) true
    if useProxy then
        client.Proxy <- WebProxy(getConfiguration "proxy_addr")
        client.Proxy.Credentials <- NetworkCredential(getConfiguration "proxy_user", getConfiguration "proxy_pass")
    else
        ()



type Link = 
    | First of int
    | Next of int
    | Last of int
    | Unknown



type LinkRecord = 
    {First: int; Next: int option; Last: int}

type ResponceType = 
    | PagedContent of int * string
    | Content of string
    | Created
    | Error

let excecuteRequest (request : IRestRequest) = 
    
    let (|Contains|_|) (s1:string) (s2:string) = 
        if s2.Contains(s1) then Some s2 else None

    let extractPage (s:string) = 
        let start = s.IndexOf("<") + 1
        let length = s.IndexOf(">") - start
        let link = s.Substring(start,length)
        
        let input = link.Substring(link.IndexOf("page") + 5)
        String(input.TakeWhile(Char.IsDigit).ToArray()) |> Int32.Parse
        

    let toLink = 
        function
        | Contains "first" x -> extractPage x |> First 
        | Contains "next" x -> extractPage x |> Next
        | Contains "last" x -> extractPage x |> Last
        | _ -> Unknown
    let toLinkRecord (l:Link seq)=
        let firsts =
            l |> Seq.choose(fun x ->
                match x with 
                | First l -> Some l
                | _ -> None) |> List.ofSeq
        let nexts =
            l |> Seq.choose(fun x ->
                match x with 
                | Next l -> Some l
                | _ -> None) |> List.ofSeq
        let lasts =
            l |> Seq.choose(fun x ->
                match x with 
                | Last l -> Some l
                | _ -> None) |> List.ofSeq
        {
            First = firsts.Head
            Next= if not nexts.IsEmpty then Some nexts.Head else None
            Last = lasts.Head
        }
    

    init()
    let response = 
        request
        |> addAuthorization
        |> client.Execute

    let linksObj = response.Headers |> Seq.find (fun x -> x.Name = "Link")
    let links = ((string)linksObj.Value).Split(',') |> Seq.map toLink |> toLinkRecord

    match response.StatusCode with
    | HttpStatusCode.OK -> Content response.Content
    | HttpStatusCode.Created -> Created
    | _ -> Error



let getTags() = 
    getRequest "/tags" Method.GET
    |> excecuteRequest
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Tag list>(x))

let GetTags() = 
    match getTags() with
    | Some x -> x
    | None -> []

let getCategories() = 
    getRequest "/categories" Method.GET
    |> excecuteRequest
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Category list>(x))

let getEntries (f:DateTime) (t:DateTime) = 
    let resource = sprintf "/entries?from=%s&to=%s&page=0" (f.ToString("yyyy-MM-dd")) (t.ToString("yyyy-MM-dd"))
    getRequest resource Method.GET
    |> excecuteRequest
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Entry list>(x))

let GetEntries (f:DateTime) (t:DateTime) = 
    match getEntries f t with
    | Some x -> x
    | None -> []


let getAccounts() = 
    getRequest "/accounts" Method.GET
    |> excecuteRequest
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Account list>(x))

let GetAccounts() = 
    match getAccounts() with
    | Some x -> x
    | None -> []

let setEntry (entry : Entry) = 
    let request = getRequest "/entries" Method.POST
    let accept = "application/json"
    request.RequestFormat <- DataFormat.Json;
    request.
        AddHeader("Content-Type", accept).
        AddBody(entry) 
    |> excecuteRequest

let SaveRecords account path file= 
    let tags = GetTags()
    let getTag (x : ReportVm) = tags |> List.filter (fun t -> t.id = x.Tag.ToString())
    let categories = getCategories()
    
    let getCategory (x : ReportVm) = 
        let tag = getTag x |> List.head
        if categories.IsSome then 
            (categories.Value
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
    
    
    let mov = Movimenti path file
    let filtered = mov |> Seq.filter (fun x -> x.Tagged)
    let entries = filtered |> Seq.map createEntry
    for entry in entries do
        setEntry entry |> ignore
    
  
