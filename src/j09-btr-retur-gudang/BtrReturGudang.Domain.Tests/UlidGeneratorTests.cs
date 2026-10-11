using System.Collections.Generic;
using BtrReturGudang.Domain.Common;
using Xunit;

namespace BtrReturGudang.Domain.Tests
{
    public class UlidGeneratorTests
    {
        [Fact]
        public void NewUlid_GeneratesExactly26Characters()
        {
            var ulid = UlidGenerator.NewUlid();

            Assert.NotNull(ulid);
            Assert.Equal(26, ulid.Length);
            Assert.True(UlidGenerator.IsValid(ulid));
        }

        [Fact]
        public void NewUlid_GeneratesValidCrockfordBase32CharactersOnly()
        {
            const string crockfordAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

            for (var i = 0; i < 50; i++)
            {
                var ulid = UlidGenerator.NewUlid();
                Assert.Equal(26, ulid.Length);

                foreach (var c in ulid)
                {
                    Assert.Contains(c, crockfordAlphabet);
                }
            }
        }

        [Fact]
        public void NewUlid_GeneratesMonotonicallyIncreasingValues()
        {
            var list = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(UlidGenerator.NewUlid());
            }

            for (var i = 1; i < list.Count; i++)
            {
                // Lexicographical ordering must be strictly greater
                var cmp = string.CompareOrdinal(list[i], list[i - 1]);
                Assert.True(cmp > 0, $"ULID at index {i} ({list[i]}) was not greater than index {i - 1} ({list[i - 1]})");
            }
        }

        [Theory]
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAV", true)]
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA", false)] // 25 chars
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAVV", false)] // 27 chars
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAI", false)] // contains 'I' (invalid Crockford)
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAL", false)] // contains 'L'
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAO", false)] // contains 'O'
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAU", false)] // contains 'U'
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValid_ValidatesCorrectly(string input, bool expected)
        {
            Assert.Equal(expected, UlidGenerator.IsValid(input));
        }
    }
}
