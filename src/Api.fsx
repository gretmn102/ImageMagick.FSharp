#r "nuget: FSharpMyExt, 2.0.0-prerelease.11"
#load "Image.fsx"
open System
open System.IO
open FsharpMyExtension
open Image

// refactor: use `FsharpMyExtension.IO.Path.Operators.(</>)` (see https://github.com/gretmn102/FsharpMyExtension/issues/23)
let (</>) x y = Path.Combine(x, y)

let convertPath =
    match Environment.OSVersion.Platform with
    | PlatformID.Unix -> "convert"
    | _ -> @"e:\msys64\mingw64\bin\convert.exe"

let convert src dst =
    Proc.startProcSimple convertPath (
        String.concat " " [
            $"\"{src}\""
            $"\"{dst}\""
        ]
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

type ConvertFolderOptions = {
    FitSize: Size option
    Dry: bool
    /// `(=) ".png"` for example
    InputFormats: string -> bool
    OutputDirectory: string option
}

let convertFolderToWebp (options: ConvertFolderOptions) dirPath =
    let dir = DirectoryInfo dirPath
    let files =
        dir.GetFiles "*.*"
        |> Array.filter (fun path ->
            options.InputFormats path.Extension
        )

    let outputDirectory =
        match options.OutputDirectory with
        | None -> dirPath
        | Some output ->
            Directory.CreateDirectory output |> ignore
            output

    files
    |> Array.iter (fun file ->
        let srcPath = file.FullName
        let dstPath =
            outputDirectory </> $"{Path.GetFileNameWithoutExtension srcPath}.webp"
        let command =
            String.concat " " [
                $"\"%s{srcPath}\""
                match options.FitSize with
                | Some fitSize ->
                    let imageSize =
                        match size srcPath with
                        | Result.Error errMsg ->
                            failwithf "%s" errMsg
                        | Result.Ok (w, h) ->
                            let imageSize = Size.create w h
                            imageSize * fitScale fitSize imageSize
                    $"-resize %d{imageSize.Width}x{imageSize.Height}!"
                | None -> ()
                $"\"%s{dstPath}\""
            ]
        printfn $"%s{convertPath} %s{command}"
        if not options.Dry then
            let statusCode = Proc.startProcSimple convertPath command
            printfn $"statusCode = %d{statusCode}"
    )
