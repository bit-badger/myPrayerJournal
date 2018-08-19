ALTER TABLE mpj.request
  ADD COLUMN "showAfter" BIGINT NOT NULL DEFAULT 0;
ALTER TABLE mpj.request
  ADD COLUMN "recurType" VARCHAR(10) NOT NULL DEFAULT 'immediate';
ALTER TABLE mpj.request
  ADD COLUMN "recurCount" SMALLINT NOT NULL DEFAULT 0;
CREATE OR REPLACE VIEW mpj.journal AS
  SELECT
    request."requestId",
    request."userId",
    (SELECT "text"
       FROM mpj.history
      WHERE history."requestId" = request."requestId"
        AND "text" IS NOT NULL
      ORDER BY "asOf" DESC
      LIMIT 1) AS "text",
    (SELECT "asOf"
       FROM mpj.history
      WHERE history."requestId" = request."requestId"
      ORDER BY "asOf" DESC
      LIMIT 1) AS "asOf",
    (SELECT "status"
       FROM mpj.history
      WHERE history."requestId" = request."requestId"
      ORDER BY "asOf" DESC
      LIMIT 1) AS "lastStatus",
    request."snoozedUntil",
    request."showAfter",
    request."recurType",
    request."recurCount"
  FROM mpj.request;