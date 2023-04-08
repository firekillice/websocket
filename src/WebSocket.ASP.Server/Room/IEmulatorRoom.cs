using System;
using Carbon.Match.New.Contact;
using Microsoft.AspNetCore.Http;
using Server.Protocols;

namespace Carbon.Match.New.Room
{
    public interface IEmulatorRoom : IDisposable
    {
        /// <summary>
        /// 玩家是否属于该房间
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        bool CheckPlayerExists(long pid);

        /// <summary>
        /// 消息排队
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="context"></param>
        /// <param name="pvpMesage"></param>
        public void QueueMessage(long pid, HttpContext context, PvPMessage pvpMesage);

        /// <summary>
        /// 玩家离开
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="context"></param>
        public void PlayerLeave(long pid, HttpContext context);

        /// <summary>
        /// 玩家进来
        /// </summary>
        /// <param name="player"></param>
        public void PlayerEnter(RoomPlayer player);
    }
}
