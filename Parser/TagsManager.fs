module TagsRuleManager

open Types
open System.IO

let private addRule rule tagId startString path=
    let fileName = path + "MappingRules.csv"
    let ruleFile = Ignored.Load(fileName)
    let newRow = [Ignored.Row(rule,tagId,startString)]
    let newRules = ruleFile.Append newRow

    File.WriteAllText (fileName, newRules.SaveToString(';'))

let AddIgnoreRule startString path=
    addRule "ignore" "" startString path

let AddTagRule tagId startString path=
    addRule "tag" tagId startString path