using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SmartStore.Collections;

namespace SmartStore.Core.Tests.Collections
{
    [TestFixture]
    public class TreeNodeTests
    {
        private TreeNode<string> BuildTestTree()
        {
            var root = new TreeNode<string>("root") { Id = "root" };

            var children = CreateNodeList(10);
            foreach (var child in children)
            {
                child.AppendRange(CreateNodeList(10, child.Value));
                root.Append(child);
            }

            return root;
        }

        private IEnumerable<TreeNode<string>> CreateNodeList(int count, string keyPrefix = "node")
        {
            for (int i = 1; i <= count; i++)
            {
                var key = keyPrefix + "-" + i;
                yield return new TreeNode<string>(key) { Id = key };
            }
        }

        [Test]
        public void Can_Deserialize_TreeNode()
        {
            var tree = BuildTestTree();
            var nodesCount = tree.SelectNodes(x => true, false).Count();

            var json = JsonConvert.SerializeObject(tree);
            var root = JsonConvert.DeserializeObject<TreeNode<string>>(json);

            var allNodes = root.SelectNodes(x => true, false).ToList();

            Assert.AreEqual(nodesCount, allNodes.Count);

            var node57 = root.SelectNodeById("node-5-7");

            Assert.IsNotNull(node57);
            Assert.AreEqual(node57.Parent.Id, "node-5");
            Assert.AreEqual(node57.Root.Value, tree.Value);
        }
    }
}
