module ToshClient
open RestSharp
open System
let upload ()=
    let client = new RestClient("http://example.com");
    // client.Authenticator = new HttpBasicAuthenticator(username, password);

    let request = new RestRequest("resource/{id}", Method.POST)
    request.AddParameter("name", "value") |> ignore // adds to POST or URL querystring based on Method
    request.AddUrlSegment("id", "123") |> ignore // replaces matching token in request.Resource

    // easily add HTTP Headers
    request.AddHeader("header", "value") |> ignore

    // execute the request
    let response = client.Execute(request)
    let content = response.Content // raw content as string

    // or automatically deserialize result
    // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
    let response2 = client.Execute(request)
    let name = response2.Data.Name

    // easy async support
    client.ExecuteAsync(request,(fun response -> printf "%s" (response.Content.ToString()))) |> ignore

    // async with deserialization
    let asyncHandle = client.ExecuteAsync(request,(fun response -> Console.WriteLine(response.Data.Name)))|> ignore

    // abort the request on demand
    asyncHandle.Abort();