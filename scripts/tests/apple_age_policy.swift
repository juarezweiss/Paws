@main
struct AppleAgePolicyTests {
    static func main() {
        let cases: [(Int?, Int?, Bool, Int32)] = [
            (nil, nil, false, 0), (nil, nil, true, 0),
            (18, nil, true, 3), (18, nil, false, 0), (18, 18, true, 3),
            (18, 17, true, 0), (-1, 17, true, 0), (13, -1, true, 0),
            (nil, 12, false, 1), (0, 12, true, 1), (nil, 13, true, 0),
            (13, 15, false, 2), (16, 17, false, 2), (13, 18, true, 0),
            (12, 17, true, 0), (17, nil, true, 0), (nil, 0, false, 1),
            (13, 13, false, 2), (19, nil, true, 3)
        ]
        for (lower, upper, confirmed, expected) in cases {
            precondition(PawsAppleAgeRangePolicy.band(lower: lower, upper: upper, confirmed: confirmed) == expected)
        }
        print("Apple age policy: \(cases.count) assertions passed")
    }
}
