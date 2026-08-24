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
  );