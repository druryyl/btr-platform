-- Add mandatory void reason metadata to BTR_Faktur.
-- Idempotent: safe to re-run when columns already exist.

IF COL_LENGTH('BTR_Faktur', 'VoidReasonCode') IS NULL
BEGIN
    ALTER TABLE BTR_Faktur
        ADD VoidReasonCode INT NOT NULL
            CONSTRAINT DF_BTR_Faktur_VoidReasonCode DEFAULT(0);
END
GO

IF COL_LENGTH('BTR_Faktur', 'VoidReasonNote') IS NULL
BEGIN
    ALTER TABLE BTR_Faktur
        ADD VoidReasonNote VARCHAR(200) NOT NULL
            CONSTRAINT DF_BTR_Faktur_VoidReasonNote DEFAULT('');
END
GO
