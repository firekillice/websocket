using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Match.Contact
{
    public class PVPCreateRoomRequest
    {
        /// <summary>
        /// 房间类型
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// PvP房间Id
        /// </summary>
        public long RoomId { get; set; }
        /// <summary>
        /// 主场玩家Id
        /// </summary>
        public long HomeId { get; set; }
        /// <summary>
        /// 客场玩家Id
        /// </summary>
        public long AwayId { get; set; }
        /// <summary>
        /// 逻辑服地址
        /// </summary>
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
