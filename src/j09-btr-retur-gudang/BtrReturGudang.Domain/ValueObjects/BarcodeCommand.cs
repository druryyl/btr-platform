using System;
using System.Collections.Generic;

namespace BtrReturGudang.Domain.ValueObjects
{
    /// <summary>
    /// Identifies hands-free desk command barcodes scanned via USB barcode reader.
    /// Used to control return intake workflows without touching keyboard/mouse.
    /// </summary>
    public enum BarcodeCommandType
    {
        None = 0,
        SwitchToQtyBagus,
        SwitchToQtyRusak,
        SaveDraft,
        ResetForm,
        CommitItem,
        SaveAndUpload
    }

    /// <summary>
    /// Value Object representing a barcode command.
    /// Evaluates scanned payloads and matches command aliases in a case-insensitive,
    /// whitespace-resilient manner.
    /// </summary>
    public sealed class BarcodeCommand : IEquatable<BarcodeCommand>
    {
        private static readonly Dictionary<string, BarcodeCommandType> CommandMap =
            new Dictionary<string, BarcodeCommandType>(StringComparer.OrdinalIgnoreCase)
            {
                // Mode switches
                { "CMD:BAGUS", BarcodeCommandType.SwitchToQtyBagus },
                { "CMD:QTY_BAGUS", BarcodeCommandType.SwitchToQtyBagus },
                { "$$BAGUS", BarcodeCommandType.SwitchToQtyBagus },
                { "BTR:QTY-BAGUS", BarcodeCommandType.SwitchToQtyBagus },
                { "BTR:QTY_BAGUS", BarcodeCommandType.SwitchToQtyBagus },

                { "CMD:RUSAK", BarcodeCommandType.SwitchToQtyRusak },
                { "CMD:QTY_RUSAK", BarcodeCommandType.SwitchToQtyRusak },
                { "$$RUSAK", BarcodeCommandType.SwitchToQtyRusak },
                { "BTR:QTY-RUSAK", BarcodeCommandType.SwitchToQtyRusak },
                { "BTR:QTY_RUSAK", BarcodeCommandType.SwitchToQtyRusak },

                // Document / action commands
                { "CMD:F9", BarcodeCommandType.SaveDraft },
                { "CMD:SIMPAN", BarcodeCommandType.SaveDraft },
                { "CMD:SIMPAN_DRAFT", BarcodeCommandType.SaveDraft },
                { "$$F9", BarcodeCommandType.SaveDraft },

                { "CMD:ESC", BarcodeCommandType.ResetForm },
                { "CMD:RESET", BarcodeCommandType.ResetForm },
                { "CMD:RESET_FORM", BarcodeCommandType.ResetForm },
                { "$$ESC", BarcodeCommandType.ResetForm },

                { "CMD:ENTER", BarcodeCommandType.CommitItem },
                { "CMD:TAMBAH", BarcodeCommandType.CommitItem },
                { "$$ENTER", BarcodeCommandType.CommitItem },

                { "CMD:F10", BarcodeCommandType.SaveAndUpload },
                { "CMD:UPLOAD", BarcodeCommandType.SaveAndUpload },
                { "$$F10", BarcodeCommandType.SaveAndUpload }
            };

        public BarcodeCommandType CommandType { get; }
        public string RawValue { get; }
        public string NormalizedValue { get; }
        public bool IsCommand => CommandType != BarcodeCommandType.None;

        public BarcodeCommand(string rawValue)
        {
            RawValue = rawValue ?? string.Empty;
            NormalizedValue = BarcodeNormalization.Normalize(RawValue);
            CommandType = TryParse(RawValue, out BarcodeCommandType cmd) ? cmd : BarcodeCommandType.None;
        }

        public BarcodeCommand(string rawValue, BarcodeCommandType commandType)
        {
            RawValue = rawValue ?? string.Empty;
            NormalizedValue = BarcodeNormalization.Normalize(RawValue);
            CommandType = commandType;
        }

        /// <summary>
        /// Attempts to parse raw barcode scanner input into a strongly-typed <see cref="BarcodeCommandType"/>.
        /// Normalizes whitespace, CR, LF, and Tab, performing case-insensitive prefix and alias matching.
        /// Returns false and sets command to None for standard product barcodes (e.g. EAN-13, SKU).
        /// </summary>
        public static bool TryParse(string rawInput, out BarcodeCommandType command)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                command = BarcodeCommandType.None;
                return false;
            }

            var normalized = BarcodeNormalization.Normalize(rawInput);
            if (string.IsNullOrEmpty(normalized))
            {
                command = BarcodeCommandType.None;
                return false;
            }

            if (CommandMap.TryGetValue(normalized, out var matchedType))
            {
                command = matchedType;
                return true;
            }

            // Fallback for Code 39 start/stop asterisks if scanner retains them
            if (normalized.Length > 2 && normalized.StartsWith("*") && normalized.EndsWith("*"))
            {
                var stripped = normalized.Substring(1, normalized.Length - 2);
                if (CommandMap.TryGetValue(stripped, out matchedType))
                {
                    command = matchedType;
                    return true;
                }
            }

            command = BarcodeCommandType.None;
            return false;
        }

        /// <summary>
        /// Attempts to parse raw barcode scanner input into a <see cref="BarcodeCommand"/> value object.
        /// </summary>
        public static bool TryParse(string rawInput, out BarcodeCommand command)
        {
            if (TryParse(rawInput, out BarcodeCommandType cmdType))
            {
                command = new BarcodeCommand(rawInput, cmdType);
                return true;
            }

            command = new BarcodeCommand(rawInput, BarcodeCommandType.None);
            return false;
        }

        /// <summary>
        /// Parses raw input into a <see cref="BarcodeCommand"/> value object.
        /// </summary>
        public static BarcodeCommand Parse(string rawInput)
        {
            return new BarcodeCommand(rawInput);
        }

        public static BarcodeCommand None => new BarcodeCommand(string.Empty, BarcodeCommandType.None);

        public bool Equals(BarcodeCommand other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CommandType == other.CommandType &&
                   string.Equals(NormalizedValue, other.NormalizedValue, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is BarcodeCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)CommandType * 397) ^ (NormalizedValue != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizedValue) : 0);
            }
        }

        public static bool operator ==(BarcodeCommand left, BarcodeCommand right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BarcodeCommand left, BarcodeCommand right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return IsCommand ? $"{CommandType} ({NormalizedValue})" : (NormalizedValue ?? string.Empty);
        }
    }
}
