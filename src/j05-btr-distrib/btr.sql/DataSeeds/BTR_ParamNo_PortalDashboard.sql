-- Run on dev after deploy if portal dashboard snapshot prefixes are missing.
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'PDR')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('PDR', '0');
GO
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'PDA')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('PDA', '0');
GO
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'PDT')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('PDT', '0');
GO
IF NOT EXISTS (SELECT 1 FROM BTR_ParamNo WHERE Prefix = 'PDB')
    INSERT INTO BTR_ParamNo (Prefix, HexVal) VALUES ('PDB', '0');
GO
