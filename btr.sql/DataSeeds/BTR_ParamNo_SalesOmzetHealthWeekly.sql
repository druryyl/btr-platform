-- Run on dev after deploy if prefix OHW is missing (SalesOmzetHealthWeeklyWriter).
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'OHW')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('OHW', '0');
GO
