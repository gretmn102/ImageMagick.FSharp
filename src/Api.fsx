#r "nuget: FSharpMyExt, 2.0.0-prerelease.11"
open System
open System.IO
open FsharpMyExtension

// refactor: use `FsharpMyExtension.IO.Path.Operators.(</>)` (see https://github.com/gretmn102/FsharpMyExtension/issues/23)
let (</>) x y = Path.Combine(x, y)

let convert src dst =
    let convertPath =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix -> "convert"
        | _ -> @"e:\msys64\mingw64\bin\convert.exe"
    Proc.startProcSimple convertPath (
        [$"\"{src}\""; $"\"{dst}\""]
        |> String.concat " "
    )

open FParsec
open FsharpMyExtension.Serialization.Deserializers.FParsec

let size (image: string) =
    let statusCode, stdout =
        Proc.startProcString "identify"
            $"-ping -format \"%%w %%h\" \"{image}\""
    if statusCode <> 0 then
        Result.Error "Some error in identify" // todo: add stderror
    else
        let p = pint32 .>> spaces .>>. pint32
        runResult p stdout

let convertFolderToWebp dry dirPath =
    let dir = DirectoryInfo dirPath
    let files = dir.GetFiles "*.png"
    files
    |> Array.iter (fun file ->
        let srcPath = file.FullName
        let dstPath =
            file.DirectoryName </> $"{Path.GetFileNameWithoutExtension srcPath}.webp"
        printfn "convert %s %s" srcPath dstPath
        if not dry then
            convert srcPath dstPath
            |> printfn "%d"
    )
