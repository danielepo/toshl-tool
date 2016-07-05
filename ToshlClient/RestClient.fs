module ToshClient

open Newtonsoft.Json
open Parser
open RestSharp
open System
open System.Net

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
//    extra: obj;
let addAuthorization (request : IRestRequest) = 
    request.AddHeader
        ("Authorization", 
         "Basic ZWYyZDAyMDktZjRiMC00NjAxLTk1NTQtOWY5MzI1NGFhNjlmNWFlYThhMjUtMWM4ZS00ZTcyLTk5NzUtNTFmMjQzNjlhNGYzOg==")

let init() = ()
//    client.Proxy <- System.Net.HttpWebRequest.DefaultWebProxy
//    client.Proxy.Credentials <- NetworkCredential("eul0856", "Dony2206!")

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
    |> Option.map (fun x -> JsonConvert.DeserializeObject<Account list>(x))

let GetAccounts() = 
    match getAccounts() with
    | Some x -> x
    | None -> []

let setEntry (entry : Entry) = 
    let jsonEntry = Newtonsoft.Json.JsonConvert.SerializeObject(entry)
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
    
    ()
