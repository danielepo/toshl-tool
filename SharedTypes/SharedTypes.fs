module SharedTypes

open System
type CatType =
    | Expence
    | Income

type ReportVm ={
    Ammount : double
    Date: DateTime
    Description: string
    Causale: int 
    Type: CatType
    Tagged: bool
    Tag:int
    Hash:string
    Category: int
    Account: int}

type RuleType = 
    | Ignore = 0
    | Tagged = 1