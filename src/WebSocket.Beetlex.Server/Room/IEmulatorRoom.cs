using System;
using BeetleX;
using Carbon.Match.Contact;

namespace Carbon.Match.Room
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
        /// <param name="session"></param>
        /// <param name="pvpMesage"></param>
        public void QueueMessage(long pid, ISession session, PVPMessage pvpMesage);

        /// <summary>
        /// 玩家离开
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="session"></param>
        public void PlayerLeave(long pid, ISession session);

        /// <summary>
        /// 玩家进来
        /// </summary>
        /// <param name="player"></param>
        public void PlayerEnter(RoomPlayer player);
    }
}
