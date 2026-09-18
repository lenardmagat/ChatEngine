SELECT "Id", COUNT(*) 
FROM "Products" 
GROUP BY "Id" 
HAVING COUNT(*) > 1;INSERT INTO Products (
    Id,
    OwnerUserId,
    ProductName,
    ProductDescription,
    BasePrice,
    Stock,
    ReservedProdcut,
    UpdatedA,
    Mode,
    ProductAvailable
  )
VALUES (
    Id:integer,
    OwnerUserId:integer,
    'ProductName:character varying',
    'ProductDescription:character varying',
    BasePrice:numeric,
    Stock:integer,
    ReservedProdcut:integer,
    'UpdatedA:timestamp with time zone',
    Mode:integer,
    ProductAvailable:integer
  );INSERT INTO Messages (
      Id,
      RoomId,
      SenderId,
      TimeStamp,
      IsEdited,
      EditedAt,
      IsDeleted,
      DeletedAt,
      Type,
      MessageText,
      TradeOfferId,
      SaleOfferId
    )
  VALUES (
      Id:integer,
      RoomId:integer,
      SenderId:integer,
      'TimeStamp:timestamp with time zone',
      IsEdited:boolean,
      'EditedAt:timestamp with time zone',
      IsDeleted:boolean,
      'DeletedAt:timestamp with time zone',
      Type:integer,
      'MessageText:text',
      TradeOfferId:integer,
      SaleOfferId:integer
    );