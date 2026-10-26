// #67: coarse classification only. Raw bounds never cross the native bridge.
enum PawsAppleAgeRangePolicy {
    static func band(lower: Int?, upper: Int?, confirmed: Bool) -> Int32 {
        if let lower, lower < 0 { return 0 }
        if let upper, upper < 0 { return 0 }
        if let lower, let upper, lower > upper { return 0 }
        if let lower, lower >= 18 { return confirmed ? 3 : 0 }
        if let upper, upper < 13 { return 1 }
        if let lower, let upper, lower >= 13 && upper < 18 { return 2 }
        return 0
    }
}
