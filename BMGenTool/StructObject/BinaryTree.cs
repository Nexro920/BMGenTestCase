#if itc
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using BMGenTool.Generate;


namespace BMGenTool.Info
{
    public class BinaryTree
    {
        private BlockNode _root;
        private int _distance;
        private string _dir;
        private SyDB _sydb;
        private string _type;
        private bool _reverse;
        private int _id;

        public BinaryTree(Block block, int distance, string dir, SyDB sydb,string type,bool reverse)
        {
            _root = new BlockNode(block);
            _distance = distance;
            _dir = dir;
            _sydb = sydb;
            _type = type;
            _reverse = reverse;
            Add(_root,_type,0);
        }

        public BinaryTree(Block block,string dir,int id,SyDB sydb)
        {
            _root = new BlockNode(block);
            _dir = dir;
            _sydb = sydb;
            _id = id;
            Add(_root);
        }

        public List<PathInfo> GetPathList(Signal sig)
        {
            List<PathInfo> list = new List<PathInfo>();
            //中序遍历二叉树，找到所有路径的终点，即所有叶子节点
            List<BlockNode> leafNode = new List<BlockNode>();
            GetLeafNode(_root, leafNode);
            //对每一叶子节点，找到从根节点到叶子节点的路径，即经过的block
            foreach (BlockNode node in leafNode)
            {
                List<Block> blockList = new List<Block>();
                blockList.Add(_root.block);
                //从起始block到终点block的路径
                Stack st = new Stack();
                blockList = this.GetNodePath(node);
                //找到此路径上沿上游方向的所有发散道岔
                List<PointInfo> pointList = this.GetPointInfo(blockList);
                //按搜索方向排列还是反方向排列
                if (_reverse)
                {
                    blockList.Reverse();
                    pointList.Reverse();
                }                
                PathInfo path = new PathInfo(blockList, pointList, sig);
                list.Add(path);
            }
            return list;
        }

        private List<Block> GetNodePath(BlockNode block)
        {
            Stack<BlockNode> st = new Stack<BlockNode>();
            if(!GetPath(_root,block.block,ref st))
            {
                return null;
            }
            List<Block> blockList = new List<Block>();
            List<BlockNode> list = new List<BlockNode>(st.ToArray());
            foreach (BlockNode node in list)
            {
                blockList.Add(node.block);
            }
            return blockList;
        }

        private bool GetPath(BlockNode root, Block node,ref Stack<BlockNode> stack)
        {
            if (null == root)
            {
                return false;
            }
            if (root.block.ID == node.ID)
            {
                stack.Push(root);
                return true;
            }
            if (GetPath(root.Normal, node, ref stack) || GetPath(root.Reverse, node, ref stack))
            {
                stack.Push(root);
                return true;
            }
            return false;
        }

        //找到二叉树的叶子节点,叶子节点的normal和reverse方向节点都为空
        private void GetLeafNode(BlockNode node, List<BlockNode> list)
        {
            if (null == node.Normal && null == node.Reverse)
            {
                list.Add(node);
            }
            else
            {
                if (null != node.Normal)
                {
                    GetLeafNode(node.Normal, list);
                }
                if (null != node.Reverse)
                {
                    GetLeafNode(node.Reverse, list);
                }
            }
        }

        //只按照normal或者reverse方向，或者两个方向都遍历，根据type类型来计算
        private void Add(BlockNode parent, string type,int length)
        {
            if (length < _distance)
            {
                if (_dir == Sys.Up)
                {
                    int nextId = 0;
                    if (type == Sys.Normal || type == Sys.Both)
                    {
                        nextId = parent.block.NextUpNBlockId;
                        if (0 != nextId)
                        {
                            Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                            parent.Normal = new BlockNode(nBlock);
                            Add(parent.Normal, type,length + nBlock.GetBlockLen());
                        }
                    }
                    if (type == Sys.Reverse || type == Sys.Both)
                    {
                        nextId = parent.block.NextUpRBlockId;
                        if (0 != nextId)
                        {
                            Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                            parent.Reverse = new BlockNode(nBlock);
                            Add(parent.Reverse, type,length + nBlock.GetBlockLen());
                        }
                    }                    
                }
                else
                {
                    int nextId = 0;                    
                    if (type == Sys.Normal || type == Sys.Both)
                    {
                        nextId = parent.block.NextDnNBlockId; 
                        if (0 != nextId)
                        {
                            Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                            parent.Normal = new BlockNode(nBlock);
                            Add(parent.Normal, type,length + nBlock.GetBlockLen());
                        }
                    }
                    if (type == Sys.Reverse || type == Sys.Both)
                    {
                        nextId = parent.block.Next_Down_Reverse_Block_ID;
                        if (0 != nextId)
                        {
                            Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                            parent.Reverse = new BlockNode(nBlock);
                            Add(parent.Reverse, type,length + nBlock.GetBlockLen());
                        }
                    }                    
                }
            }
        }

        private void Add(BlockNode parent)
        {
            int signalId = this.GetBlockSignal(parent.block, _dir);
            if (0 == signalId || _id == signalId)
            {
                if (_dir == Sys.Up)
                {
                    int nextId = parent.block.NextUpNBlockId;
                    if (0 != nextId)
                    {
                        Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                        parent.Normal = new BlockNode(nBlock);
                        Add(parent.Normal);
                    }
                    nextId = parent.block.NextUpRBlockId;
                    if (0 != nextId)
                    {
                        Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                        parent.Reverse = new BlockNode(nBlock);
                        Add(parent.Reverse);
                    }
                }
                else
                {
                    int nextId = parent.block.NextDnNBlockId;
                    if (0 != nextId)
                    {
                        Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                        parent.Normal = new BlockNode(nBlock);
                        Add(parent.Normal);
                    }
                    nextId = parent.block.Next_Down_Reverse_Block_ID;
                    if (0 != nextId)
                    {
                        Block nBlock = (Block)Sys.GetNode(nextId, _sydb.blockInfoList.Cast<Basic>().ToList());
                        parent.Reverse = new BlockNode(nBlock);
                        Add(parent.Reverse);
                    }
                }
            }
        }

        //根据进路中的block列表，计算进路中的所有point
        public List<PointInfo> GetPointInfo(List<Block> blockList)
        {
            List<PointInfo> list = new List<PointInfo>();
            for (int i = 0; i < blockList.Count - 1; i++)
            {
                Block preBlock = blockList[i];
                Block nextBlock = blockList[i + 1];
                if (preBlock.PointId == nextBlock.PointId && 0 != preBlock.PointId)
                {
                    Point point = (Point)Sys.GetNode(preBlock.PointId, _sydb.pointInfoList.Cast<Basic>().ToList());
                    //string pos = "";
                    ////BMGR-0039 ??
                    //if (_dir == Sys.Up)
                    //{
                    //    if (preBlock.NextUpNBlockId == nextBlock.ID && nextBlock.NextDnNBlockId == preBlock.ID)
                    //    {
                    //        pos = "Normal";
                    //    }
                    //    else
                    //    {
                    //        pos = "Reverse";
                    //    }
                    //}
                    //else
                    //{
                    //    if (preBlock.NextDnNBlockId == nextBlock.ID && nextBlock.NextUpNBlockId == preBlock.ID)
                    //    {
                    //        pos = "Normal";
                    //    }
                    //    else
                    //    {
                    //        pos = "Reverse";
                    //    }
                    //}
                    string dir = _dir;
                    //确定方向是否反转
                    if (_reverse)
                    {
                        if (_dir == Sys.Up)
                        {
                            dir = Sys.Dn;
                        }
                        else
                        {
                            dir = Sys.Up;
                        }
                    }
                    //PointInfo pi = new PointInfo(point, pos);
                    //list.Add(pi);
                }
            }

            return list;
        }

        //计算block上包含的某方向的signal，如果有，返回signal id，如果没有，返回0
        //同一block上可能有多个信号机，若为UP方向则取第一个，若为DOWN方向，则取最后一个
        private int GetBlockSignal(Block block, string dir)
        {
            int id = 0;
            List<Signal> sigList = new List<Signal>();
            foreach (Signal signal in _sydb.signalInfoList)
            {
                if (signal.TrackId == block.TrackId && signal.Direction == dir)
                {
                    if ((block.KpBegin <= signal.SignalKp.KpRealValue && block.KpEnd >= signal.SignalKp.KpRealValue)
                        || (block.KpBegin >= signal.SignalKp.KpRealValue && block.KpEnd <= signal.SignalKp.KpRealValue))
                    {
                        sigList.Add(signal);
                    }
                }

            }
            bool isReverse = false;
            //此track上KP逆序
            if (block.KpBegin > block.KpEnd)
            {
                isReverse = true;
            }
            if (1 == sigList.Count)
            {
                id = sigList[0].ID;
            }
            else if (1 < sigList.Count)
            {
                //若此block上存在2个以上信号时，取此方向上的第一个
                //找到block上KP最大和最小的信号机
                Signal maxKpSig = null;
                Signal minKpSig = null;
                foreach (Signal node in sigList)
                {
                    if (null == maxKpSig)
                    {
                        maxKpSig = node;
                    }
                    else
                    {
                        if (maxKpSig.SignalKp.KpRealValue < node.SignalKp.KpRealValue)
                        {
                            maxKpSig = node;
                        }
                    }
                    if (null == minKpSig)
                    {
                        minKpSig = node;
                    }
                    else
                    {
                        if (minKpSig.SignalKp.KpRealValue > node.SignalKp.KpRealValue)
                        {
                            minKpSig = node;
                        }
                    }
                }
                //若KP为正序，则UP方向时取KP最小值，DOWN方向取KP最大值；反之亦然
                if (!isReverse)
                {
                    if (dir == Sys.Up)
                    {
                        id = minKpSig.ID;
                    }
                    else
                    {
                        id = maxKpSig.ID;
                    }
                }
                else
                {
                    if (dir == Sys.Up)
                    {
                        id = maxKpSig.ID;
                    }
                    else
                    {
                        id = minKpSig.ID;
                    }
                }
            }
            return id;
        }
    }

    public class BlockNode
    {
        private Block _block;
        private BlockNode _normal;
        private BlockNode _reverse;
        public Block block
        {
            get { return _block; }
        }
        public BlockNode Normal
        {
            get { return _normal; }
            set { _normal = value; }
        }
        public BlockNode Reverse
        {
            get { return _reverse; }
            set { _reverse = value; }
        }
        public BlockNode(Block block)
        {
            _block = block;
        }
    }
}
#endif