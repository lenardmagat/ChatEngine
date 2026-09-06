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
public record AcceptItemDTO(
    OfferTye OfferType,
    string ParentOfferId,
    string ItemId
);
public record CounterOfferDTO(
    OfferTye OfferType,
    string ParentOfferId,
    string ItemId,
    SaleProposedDTO? SalePayload,
    TradeProposedDTO? Tradepayload
);