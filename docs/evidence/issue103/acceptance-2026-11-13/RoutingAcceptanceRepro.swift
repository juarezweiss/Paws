import Foundation
@main enum AcceptanceRoutingRepro {
 static func main() {
  var rows = [[String: Any]]()
  let featureSets: [(String,[String])] = [
   ("parental-update-only",["significantAppChangeRequiresParentalConsent"]),
   ("adult-update-only",["significantAppChangeRequiresAdultNotification"]),
   ("both-update-only",["significantAppChangeRequiresParentalConsent","significantAppChangeRequiresAdultNotification"]),
   ("ordinary-no-features",[]),
   ("required-range",["declaredAgeRangeRequired"])
  ]
  for (name, features) in featureSets {
   let required = features.contains("declaredAgeRangeRequired")
   let pendingActualChange = false // Exact shipping registry is empty.
   let actual = PawsAccessWorkflow.needsRange(required: required, checkpoint: false,
      userInitiated: false, regulated: !features.isEmpty, pendingSignificantChange: pendingActualChange)
   let expected = required
   rows.append(["scenario":name,"features":features,"pendingActualChange":pendingActualChange,
    "expectedRangeRequest":expected,"actualRangeRequest":actual,"passed":actual == expected])
  }
  let failures = rows.filter { ($0["passed"] as? Bool) == false }.count
  let result: [String:Any] = ["testKind":"deterministic host Swift routing reproduction; not Apple runtime or Sandbox",
   "rows":rows,"total":rows.count,"failed":failures,"passed":rows.count-failures,
   "nativeApiCallsPerformed":false,"appleSandboxExecuted":false]
  let data = try! JSONSerialization.data(withJSONObject:result,options:[.prettyPrinted,.sortedKeys])
  print(String(decoding:data,as:UTF8.self))
  exit(failures == 0 ? 0 : 1)
 }
}
