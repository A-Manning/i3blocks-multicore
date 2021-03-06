open System.Diagnostics

let  lowThreshold = 25 // cpu values below this are "low"
let highThreshold = 65 // cpu values above this are "high"

let  lowColor = "#00FF00" // the color for low cpu usage
let  medColor = "#FFD500" // the color for medium cpu usage
let highColor = "#FF0000" // The color for high cpu usage

type encoding =
    | Dec // decimal encoding (0-9)
    | Hex // hex encoding (0-F)
    | Bar of int // unicode bars
                 // May be somewhat buggy.
                 // The argument is a scaling factor.

let encoding = Dec
// let encoding = Hex
// let encoding = Bar 11000

// encodes a cpu core usage in the (0-99) range
// as a pango formatted string with a specified text color
let pangoColor (color:string) (usage:int) : string =
    match encoding with
    | Dec ->
        let usageAsDecimal = (usage / 10).ToString()
        sprintf "<span foreground=\"%s\">%s</span>" color usageAsDecimal
    | Hex ->
        let usageAsHex = (usage / 16).ToString("X")
        sprintf "<span foreground=\"%s\">%s</span>" color usageAsHex
    | Bar scaling ->
        let rise = scaling * usage / 100
        sprintf "<span foreground=\"%s\" rise=\"%d\">_</span>" color rise

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
                |> abs // a bug in .NET means that sometimes, a value like -0.5 can be reported.
                |> floor
                |> int32


    if usage >= 100 then failwith "core usage can't be above 100%!"
    elif usage < 0 then failwith "core usage can't be below 0%!" else

    let color =
        if usage > highThreshold then highColor
        elif usage >= lowThreshold then medColor
        else lowColor

    usage
    |> pangoColor color
    |> printf "%s"


coreMonitors |> Array.iter printUsage
