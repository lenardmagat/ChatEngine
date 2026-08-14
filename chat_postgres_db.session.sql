ALTER TABLE "Messages" 
ADD COLUMN IF NOT EXISTS "TradeOfferId" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "Type" INTEGER NOT NULL DEFAULT 0;INSERT INTO OutboxEntries (Id, EntityType, EntityId, CreatedAt, ProcessedAt)
VALUES (
    Id:integer,
    EntityType:integer,
    EntityId:integer,
    'CreatedAt:timestamp with time zone',
    'ProcessedAt:timestamp with time zone'
  );