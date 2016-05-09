module ToshClient
open RestSharp
open System
type TagCount = {
    entries:int;
    unsorted_entries:int;
    budgets:int
}

type Tag={
  id:string;
  name:string;
  name_override:bool;
  modified: DateTime;
  ``type``: string;
  category: string;
  count: TagCount;
  deleted: bool;
  transient_created: DateTime;
  transient_valid_till: string;
  meta_tag: bool;
  extra: obj
}
type CategoryCount = {
    entries: int;
    income_entries: int;
    expense_entries: int;
    tags_used_with_category: int;
    income_tags_used_with_category: int;
    expense_tags_used_with_category: int;
    tags: int;
    income_tags: int;
    expense_tags: int;
    budgets: int;
}
//type CategoryType =
//    | expense
//    | income
//    | system
type Category = {
  id: string;
  name:string;
  modified: DateTime;
  ``type``: string;
  deleted: bool;
  name_override:bool;
  transient_created:DateTime;
  transient_valid_till:string;
  counts:CategoryCount;
  extra: obj;
}
type Currency = {
    code: string;
}

type Location = {
    id: string;
    latitude: double;
    longitude: double;
}
type Repeat = {
    id: string;
    start: DateTime;
    ``end``: DateTime;
    frequency: string;
    interval: int;
    count: int;
    byday: string;
    bymonthday: string;
    bysetps: string;
    iteration: double;
}
type Transaction = {
    id: string;
    account: string;
    currency: Currency;
}
type Image = {
    id: string;
    path: string;
    status: string;
}
type Reminder = {
    period: string;
    number: int;
    at: string;
}

type Entry = {
    amount: double;
    currency: Currency;
    date: DateTime;
    desc: string;
    account: string;
    category: string;
    tags: string list
//    location: Location;
//    repeat: Repeat;
//    transaction: Transaction;
//    images: Image list;
//    reminders: Reminder list;
    completed: bool;
//    extra: obj;
}
    
let addAuthorization (request:RestRequest) = 
    request.AddHeader("Authorization", "Basic ZWYyZDAyMDktZjRiMC00NjAxLTk1NTQtOWY5MzI1NGFhNjlmNWFlYThhMjUtMWM4ZS00ZTcyLTk5NzUtNTFmMjQzNjlhNGYzOg==") |> ignore


let client = new RestClient("https://api.toshl.com");
let excecuteRequest (request:RestRequest) =
    
    addAuthorization request

    // execute the request
    let response = client.Execute(request)
    response.Content // raw content as string

let getTags ()=
    // client.Authenticator = new HttpBasicAuthenticator(username, password);
    let request = new RestRequest("/tags", Method.GET)
    let content = excecuteRequest request
    Newtonsoft.Json.JsonConvert.DeserializeObject<Tag list>(content)

let getCategories()=
    // client.Authenticator = new HttpBasicAuthenticator(username, password);
    let request = new RestRequest("/categories", Method.GET)
    let content = excecuteRequest request
    Newtonsoft.Json.JsonConvert.DeserializeObject<Category list>(content)
    
let setEntry amount date description tag category =
    let entry = {
        amount = amount;
        currency = {code = "EUR";};
        date = date;
        desc = description;
        account = "";
        category = category;
        tags = [tag];
        completed = true;
    }
    let request = new RestRequest("/entries",Method.POST)
    let jsonEntry = Newtonsoft.Json.JsonConvert.SerializeObject(entry)
    request.AddBody(jsonEntry) |> ignore
    excecuteRequest request

