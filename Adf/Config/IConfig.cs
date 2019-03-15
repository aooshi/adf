using System;

namespace Adf.Config
{
    /// <summary>
    /// 配置文件接口
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// 配置变更事件
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// 获取配置文件名称
        /// </summary>
        string FileName
        {
            get;
        }

        /// <summary>
        /// 获取本地配置文件路径
        /// </summary>
        string FilePath
        {
            get;
        }

        /// <summary>
        /// 本地配置文件是否存在
        /// </summary>
        bool FileExist
        {
            get;
        }
        
        /// <summary>
        /// 获取配置项总数
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// 配置文件是否已修改
        /// </summary>
        /// <returns></returns>
        bool CheckModifyed();

        /// <summary>
        /// 重新载入配置文件
        /// </summary>
        void Reload();
        
        /// <summary>
        /// 是否已启用变更监控
        /// </summary>
        bool IsWatcher
        {
            get;
        }

        /// <summary>
        /// 获取某一配置值
        /// </summary>
        /// <param name="name"></param>
        /// <returns>exists return value, no exists return null</returns>
        object this[string name]
        {
            get;
        }
                
        /// <summary>
        /// 是否存在某一配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Exists(string name);

        /// <summary>
        /// 尝试获取某一配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGet(string name, out object value);
    }
}
