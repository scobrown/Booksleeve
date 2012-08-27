using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SortedSets // http://redis.io/commands#sorted_set
    {
        [Test]
        public void Range() // http://code.google.com/p/booksleeve/issues/detail?id=12
        {
            using(var conn = Config.GetUnsecuredConnection())
            {
                const double value = 634614442154715;
                conn.SortedSets.Add(3, "zset", "abc", value);
                var range = conn.SortedSets.Range(3, "zset", 0, -1);

                Assert.AreEqual(value, conn.Wait(range).Single().Value);
            }
        }


        [Test]
        public void ZInterStore()
        {
            using (var conn = Config.GetUnsecuredConnection())
            {
                conn.Keys.Remove(3, new[] { "A", "B", "C" });

                conn.SortedSets.Add(3, "A", "OBJ1", 1);
                conn.SortedSets.Add(3, "B", "OBJ1", 1);

                var intersectAndStore = conn.SortedSets.ZIntersectAndStore(3, "C", new[] { "A", "B" });
                Assert.AreEqual(1, conn.Wait(intersectAndStore));

                var range = conn.SortedSets.Range(3, "C", 0, 2);
                var pair = conn.Wait(range).Single();
                Assert.AreEqual("OBJ1", Encoding.UTF8.GetString(pair.Key));
                Assert.AreEqual(2.0, pair.Value); // SUM

                intersectAndStore = conn.SortedSets.ZIntersectAndStore(3, "C", new[] { "A", "B" });
                Assert.AreEqual(1, conn.Wait(intersectAndStore));
                range = conn.SortedSets.Range(3, "C", 0, 6);
                pair = conn.Wait(range).Single();
                Assert.AreEqual("OBJ1", Encoding.UTF8.GetString(pair.Key));
                Assert.AreEqual(2.0, pair.Value); // SUM
            }
        }

        [Test]
        public void ZUnionStore()
        {
            using (var conn = Config.GetUnsecuredConnection())
            {
                conn.Keys.Remove(3, new[] { "A", "B", "C" });

                conn.SortedSets.Add(3, "A", "OBJ1", 1);
                conn.SortedSets.Add(3, "B", "OBJ1", 2);
                conn.SortedSets.Add(3, "B", "OBJ2", 1);

                var unionAndStore = conn.SortedSets.ZUnionAndStore(3, "C", new[] { "A", "B" });
                Assert.AreEqual(2, conn.Wait(unionAndStore));

                var range = conn.SortedSets.Range(3, "C", 0, 2);
                var pairs = conn.Wait(range);
                Assert.AreEqual(2, pairs.Length);

                var dict = pairs.ToDictionary(x => Encoding.UTF8.GetString(x.Key), x => x.Value);
                Assert.IsTrue(dict.ContainsKey("OBJ1"));
                Assert.IsTrue(dict.ContainsKey("OBJ2"));
                Assert.AreEqual(3.0, dict["OBJ1"]);
                Assert.AreEqual(1.0, dict["OBJ2"]);
            }
        }
    }
}
