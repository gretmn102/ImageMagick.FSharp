// refactor: use FsharpMyExtension.Graphics.Size (see https://github.com/gretmn102/FsharpMyExtension/issues/24)
type Size =
    {
        Width: int
        Height: int
    }
    static member (*) ((this: Size), (scale: float)) =
        let apply (side: int) =
            int (float side * scale)
        {
            Width = apply this.Width
            Height = apply this.Height
        }
    static member (*) ((scale: float), (this: Size)) =
        this * scale

// refactor: use FsharpMyExtension.Graphics.Size (see https://github.com/gretmn102/FsharpMyExtension/issues/24)
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Size =
    let create w h = {
        Width = w
        Height = h
    }

// refactor: use FsharpMyExtension.Graphics.fitScale (see https://github.com/gretmn102/FsharpMyExtension/issues/25)
/// Вычисляет коэффициент масштаба, необходимый, чтобы вписать изображение в указанную область
let fitScale (viewportSize: Size) (imageSize: Size) =
    let dw = imageSize.Width - viewportSize.Width
    let dh = imageSize.Height - viewportSize.Height

    if dw > dh then
        if dw > 0 then
            float viewportSize.Width / float imageSize.Width
        else
            1.0
    else
        if dh > 0 then
            float viewportSize.Height / float imageSize.Height
        else
            1.0
