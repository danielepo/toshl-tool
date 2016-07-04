module ToshClient

open RestSharp
open System
open System.Net
open Newtonsoft.Json
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

let getRequest (resource:string) (``method``:Method) = 
    RestRequest(resource, ``method``)

//    extra: obj;
let addAuthorization (request : IRestRequest) = 
    request.AddHeader
        ("Authorization", 
         "Basic ZWYyZDAyMDktZjRiMC00NjAxLTk1NTQtOWY5MzI1NGFhNjlmNWFlYThhMjUtMWM4ZS00ZTcyLTk5NzUtNTFmMjQzNjlhNGYzOg==")     

let init() = ()
//    client.Proxy <- System.Net.HttpWebRequest.DefaultWebProxy
//    client.Proxy.Credentials <- NetworkCredential("eul0856","Dony2206!")

let excecuteRequest (request : IRestRequest) = 
    init()
    let response =
        request 
        |> addAuthorization
        |> client.Execute
    
    match response.StatusCode with
    | HttpStatusCode.OK -> Some response.Content 
    | _ -> None

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

let getAccounts() = 
    getRequest "/accounts" Method.GET 
    |> excecuteRequest 
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Category list>(x))


let setEntry (entry:Entry)= 
    
    let jsonEntry = Newtonsoft.Json.JsonConvert.SerializeObject(entry)
    let request = getRequest "/entries" Method.POST
    request.AddHeader("Accept","application/json").AddBody(jsonEntry)
    |>  excecuteRequest

open Parser
open Types

let BuildEntryList(path:string) =
    let movimenti = loadMovimenti path
    let tags = getTags()
    let movimentiToEntry (x:Movement):Entry = 
            let rid = "13106"
            let euro = {code="EUR"}
            let mte (m:Record) a=
                let tag = tags |> Option.map (fun y -> y |> List.find (fun k -> k.id = m.Tag.ToString()))
                        
                {
                amount = a; 
                currency = euro;
                date = m.Date.ToString("yyyy-MM-dd");
                desc = m.Description;
                account = rid;
                category = if tag.IsSome then tag.Value.category else "0";
                tags = [m.Tag.ToString()];
                completed = true}

            match x with
            | Movement.Expence e -> mte e (-e.Ammount) 
            | Movement.Income i -> mte i i.Ammount
            
    movimenti |> 
        Seq.map movimentiToEntry

let SaveEntries path =
    let entries = BuildEntryList path |> List.ofSeq
    let rec save ee saved error=
        match ee with
        | e::es -> match setEntry e with 
                    | Some v -> save es (e::saved) error
                    | None -> save es saved (e::error)
        | [] -> saved|>Seq.ofList,error|>Seq.ofList

    save entries [] []