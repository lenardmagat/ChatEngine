namespace ChatSystem.DTOs;
public record SaleProposedDTO(
    int QuantityRequested,
    decimal ProposedPricePerunit
);
public record TradeProposedDTO(
    string ItemOffered
);
public record ProposedItemDTO(
    OfferTye Offer,
    string ItemId,
    SaleProposedDTO? SalePayload,
    TradeProposedDTO? Tradepayload
);