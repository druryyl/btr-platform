-- Run on dev after deploy if prefix SO is missing (SalesOmzetWriter).
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'SO')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('SO', '0');
GO
