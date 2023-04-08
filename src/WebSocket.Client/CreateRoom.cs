namespace WSClient
{
    public class CreateRoomRequestDto
    {
        /// <summary>
        /// 房间类型
        /// </summary>
        public string Type { get; set; }
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
    }

    public class CreateRoomResponseDto
    {
        /// <summary>
        /// 仿真器房间Id
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// PvP节点地址
        /// </summary>
        public string MatchHost { get; set; }
    }
}
