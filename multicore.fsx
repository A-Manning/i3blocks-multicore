open System.Diagnostics

let  lowThreshold = 25 // cpu values below this are "low"
let highThreshold = 65 // cpu values above this are "high"

let  lowColor = "#00FF00" // the color for low cpu usage
let  medColor = "#FFD500" // the color for medium cpu usage
let highColor = "#FF0000" // The color for high cpu usage

type encoding =
    | Dec // decimal encoding (0-9)
    | Hex // hex encoding (0-F)
    | Bar of int * string // unicode bars
                 // May be buggy and not work with some fonts.
                 // The first argument is a scaling factor.
                 // Adjust the scaling factor to zero the bars.
                 // The second argument is the background color.

let encoding = Dec
// let encoding = Hex
// let encoding = Bar (11000, "#000000")

let encodeDec (usage:int) : string =
    (usage / 10).ToString()

let encodeHex (usage:int) : string =
    (usage / 16).ToString("X")

let pangoColorBar (color:string) (usage:int) (scaling:int) (bgcolor:string)
    : string =

    let rise = scaling * usage / 100
    sprintf "<span foreground=\"%s\" background=\"%s\" rise=\"%d\">█</span>"
            bgcolor
            color
            rise

// encodes a cpu core usage in the (0-99) range
// as a pango formatted string with a specified text color
let pangoColor (color:string) (usage:int) : string =
    match encoding with
    | Dec -> sprintf "<span foreground=\"%s\">%s</span>" color (encodeDec usage)
    | Hex -> sprintf "<span foreground=\"%s\">%s</span>" color (encodeHex usage)
    | Bar(scaling, bgcolor) -> pangoColorBar color usage scaling bgcolor

// get the number of cores
let ncores = System.Environment.ProcessorCount

// gets a monitor for the nth logical core
let getCoreMonitor (n:int) : PerformanceCounter =
    new PerformanceCounter("Processor", "% Processor Time", n.ToString())

let coreMonitors : PerformanceCounter [] =
    Array.init ncores getCoreMonitor
    // get the values, and discard them.
    // The next time they are used, they will be correct.
    |> Array.map (fun monitor -> ignore (monitor.NextValue()); monitor)

// sleep for 1 second, so that the values are accurate.
System.Threading.Thread.Sleep(1000);

// print the usage for one core.
let printUsage (monitor:PerformanceCounter) : unit =
    // usage as an integer
    let usage = monitor.NextValue()
                |> floor
                |> int32

    if usage >= 100 then failwith "core usage can't be above 100%!" else

    let color =
        if usage > highThreshold then highColor
        elif usage >= lowThreshold then medColor
        else lowColor

    usage
    |> pangoColor color
    |> printf "%s"


coreMonitors |> Array.iter printUsage
