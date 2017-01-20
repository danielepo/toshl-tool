module DataAccessLayer
open Microsoft.Azure // Namespace for CloudConfigurationManager
open Microsoft.WindowsAzure.Storage // Namespace for CloudStorageAccount
open Microsoft.WindowsAzure.Storage.Table // Namespace for Table storage types

type Record = 
    { Date : System.DateTime
      Ammount : float
      Description : string
      Tag : int list
      Category : int }

type MovementEntity(month : string, hash : string, record : Record option) = 
    inherit TableEntity(partitionKey = month, rowKey = hash)
        
    member val Date = 
        
        if record.IsSome then record.Value.Date
        else System.DateTime.MinValue
        with get,set

    member val Category = 
        if record.IsSome then record.Value.Category
        else 0
        with get,set
    
    member val Ammount = 
        if record.IsSome then record.Value.Ammount
        else 0.0
        with get,set
    
    member val Description = 
        if record.IsSome then record.Value.Description
        else ""
        with get,set
    
    member val Tag = 
        if record.IsSome then record.Value.Tag
        else []
        with get,set
    member this.ToString() = 
        sprintf "%s\n%f\n%s\n%s" this.Description this.Ammount this.PartitionKey this.RowKey
    new() = MovementEntity(null, null, None)
    


module Storage = 
    // Parse the connection string and return a reference to the storage account.
    let storageAccount =
#if COMPILED 
        "StorageConnectionString"
        |> CloudConfigurationManager.GetSetting
        |> CloudStorageAccount.Parse
#else
        "UseDevelopmentStorage=true;"    |> CloudStorageAccount.Parse
#endif
        
    
    let private tableClient = storageAccount.CreateCloudTableClient()
    
    let getTable name = 
        // Retrieve a reference to the table.
        let table = tableClient.GetTableReference(name)
        // Create the table if it doesn't exist.
        table.CreateIfNotExists() |> ignore
        table
    
    let insert (row : 'a) (table : CloudTable) = 
        let insertOperation = TableOperation.Insert(row)
        // Execute the insert operation.
        table.Execute(insertOperation)
    
    let queryByPartitionKey<'a> name = 
        (new TableQuery<'a>()).Where <| TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, name)
    let getAllRows (table : CloudTable) (query : TableQuery<'a>) = 
        // Print the fields for each customer.
        table.ExecuteQuery<'a>(query)

module MovementSaver = 
    open Storage
    
    let getHash (text : string) =

        let builder = new System.Text.StringBuilder()
        use sha = System.Security.Cryptography.SHA256Managed.Create()
        let enc = System.Text.Encoding.UTF8
        let result = sha.ComputeHash(enc.GetBytes(text))
        result |> Array.iter (fun b -> builder.Append(b.ToString("x2")) |> ignore)
        builder.ToString()

    let private table() = getTable "Records"
    let insertAMovement (record : Record) = 
        let movement = new MovementEntity(record.Date.ToString("yyyy-MM"), getHash (record.Description), Some record)
        printf "%s\n" (movement.ToString())
        let result = table() |> insert movement
        ()

    let getMonth (date:System.DateTime) =
        
        (date.Date.ToString("yyyy-MM"))
        |> queryByPartitionKey<MovementEntity> 
        |> getAllRows (table())
    
    let getEntity (date:System.DateTime) (description) = 
        // TODO Migliorare le query per prendere solo uno
        getMonth date 
        |> Seq.filter (fun x -> x.Description = description)
        |> fun x -> if x |> Seq.length = 1 then Some (Seq.head x) else None

let table = Storage.getTable "Records"

table.ExecuteQuery<MovementEntity>(new TableQuery<MovementEntity>())