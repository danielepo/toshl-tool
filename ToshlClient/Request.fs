module Request 

open RestSharp
open System
open System.Net
open System.Linq

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

let get (resource : string) (``method`` : Method) = RestRequest(resource, ``method``)
let setFormat format (request:IRestRequest) =
    request.RequestFormat <- format
    request
let setHeader header value (request:IRestRequest)=
        request.AddHeader(header, value)
let setBody body (request:IRestRequest)=
    request.AddBody(body) 

let excecute (request : IRestRequest) = 
    let client = new RestClient("https://api.toshl.com")
        
    let getConfiguration (key:string) =
        System.Configuration.ConfigurationManager.AppSettings.Get(key)

    let existsConfiguration (key:string) =
        getConfiguration key <> null
    
    let init() = 
        let useProxy = ["proxy_addr";"proxy_user";"proxy_pass"] |> List.map existsConfiguration |> List.fold (&&) true
        if useProxy then
            client.Proxy <- WebProxy(getConfiguration "proxy_addr")
            client.Proxy.Credentials <- NetworkCredential(getConfiguration "proxy_user", getConfiguration "proxy_pass")
        else
            ()

    let addAuthorization (request : IRestRequest) = 
        request.AddHeader
            ("Authorization", getConfiguration "auth_token")

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
    

    let getLinks (response:IRestResponse) =
        let linksObj = 
            if response.Headers |> Seq.exists (fun x -> x.Name ="Link") 
            then Some (response.Headers |> Seq.find (fun x -> x.Name ="Link"))
            else None
        response, linksObj 
        |> Option.map (fun x -> 
            ((string)x.Value).Split(',') |> Seq.map toLink |> toLinkRecord)
            
    let execute (response:IRestResponse, links: LinkRecord option)= 
        let hasNext = links.IsSome && links.Value.Next.IsSome
        let isLast = links.IsSome && links.Value.Next.IsNone 

        match response.StatusCode with
        | HttpStatusCode.OK when hasNext -> PagedContent (links.Value.Next.Value, response.Content)
        | HttpStatusCode.OK when isLast -> Content response.Content
        | HttpStatusCode.Created -> Created
        | _ -> Error

    init() 
    request
    |> addAuthorization
    |> client.Execute
    |> getLinks
    |> execute
