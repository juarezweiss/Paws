#if PAWS_APPLE_AGE_RANGE
import Foundation
import UIKit
import DeclaredAgeRange

// #67: native Apple age signals only. No account mutations, raw-age storage or logs.
@MainActor
private enum PawsAppleAgeRange {
    static var presenting = false
    static func check(_ request: Int64, _ receive: @escaping @convention(c) (Int64, Int32) -> Void) async {
        // Earlier systems retain ordinary play/free hints; no location/age is inferred.
        guard #available(iOS 26.2, *) else { receive(request, 0); return }
        do {
            let required: Bool
            if #available(iOS 26.4, *) {
                let features = try await AgeRangeService.shared.requiredRegulatoryFeatures
                required = features.contains(.declaredAgeRangeRequired)
            } else {
                required = try await AgeRangeService.shared.isEligibleForAgeFeatures
            }
            receive(request, required ? 2 : 1)
        } catch { receive(request, 0) }
    }
    @available(iOS 26.0, *)
    static func confirmed(_ declaration: AgeRangeService.AgeRangeDeclaration?) -> Bool {
        guard let declaration else { return false }
        if #available(iOS 26.5, *), declaration == .confirmed { return true }
        if #available(iOS 26.2, *) {
            switch declaration {
            case .checkedByOtherMethod, .guardianCheckedByOtherMethod,
                 .governmentIDChecked, .guardianGovernmentIDChecked, .paymentChecked, .guardianPaymentChecked:
                return true
            default: return false
            }
        }
        return false
    }
    static func request(_ request: Int64, _ receive: @escaping @convention(c) (Int64, Int32) -> Void) async {
        guard #available(iOS 26.2, *), !presenting,
              let window = UIApplication.shared.delegate?.window ?? nil,
              window.isKeyWindow, !window.isHidden,
              window.windowScene?.activationState == .foregroundActive,
              let root = window.rootViewController, root.presentedViewController == nil else {
            receive(request, 0); return
        }
        presenting = true
        let band: Int32
        do {
            let response = try await AgeRangeService.shared.requestAgeRange(ageGates: 13, 18, in: root)
            switch response {
            case .sharing(let range):
                band = PawsAppleAgeRangePolicy.band(lower: range.lowerBound, upper: range.upperBound,
                    confirmed: confirmed(range.ageRangeDeclaration))
            case .declinedSharing: band = 0
            @unknown default: band = 0
            }
        } catch { band = 0 }
        presenting = false
        receive(request, band)
    }
}
@_cdecl("PawsAgeRangeCheck")
public func pawsAgeRangeCheck(_ request: Int64, _ receive: @escaping @convention(c) (Int64, Int32) -> Void) {
    Task { @MainActor in await PawsAppleAgeRange.check(request, receive) }
}
@_cdecl("PawsAgeRangeRequest")
public func pawsAgeRangeRequest(_ request: Int64, _ receive: @escaping @convention(c) (Int64, Int32) -> Void) {
    Task { @MainActor in await PawsAppleAgeRange.request(request, receive) }
}
#endif
