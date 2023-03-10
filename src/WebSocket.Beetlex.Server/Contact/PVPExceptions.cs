using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Match.Contact
{
    /// <summary>
    /// 异常集合
    /// </summary>
    public class PVPExceptions
    {
        /// <summary>
        /// 缺少HTTP的Header参数
        /// </summary>
        public const string HEADER_VALUE_MISSING = "header_value_missing";
        /// <summary>
        /// 房间不存在
        /// </summary>
        public const string SPECIFIC_ROOM_NOT_EXISTS = "room_not_exists";
        /// <summary>
        /// 玩家不在该房间
        /// </summary>
        public const string PLAYER_NOT_IN_SPECIFIC_ROOM = "player_not_in_the_room";
        /// <summary>
        /// 创建玩家失败
        /// </summary>
        public const string FAIL_TO_CREATE_PLAYER = "fail_to_create_player";
    }
}
