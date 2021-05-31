﻿using System.ComponentModel;
using System.Threading.Tasks;
using Platformex;

namespace Siam.MemoContext
{
    /// <summary>
    /// Идентификатор Памятки
    /// </summary>
    public class MemoId : Identity<MemoId>
    {
        public MemoId(string value) : base(value) {}
    }
    
    /*************
    ** Команды **
    *************/
    
    /// <summary>
    /// Обновить памятку
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="Document">Документ</param>
    [Description("Обновить памятку")]
    public record UpdateMemo(MemoId Id, MemoDocument Document) : ICommand<MemoId>;
    
    /// <summary>
    /// Подписать памятку
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="UserId">Пользователь, подписавший документ</param>
    [Description("Подписать памятку")]
    public record SignMemo(MemoId Id, string UserId) : ICommand<MemoId>;
    
    /// <summary>
    /// Подтвердить подписание памятки
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    [Description("Подтвердить подписание памятки")]
    public record ConfirmSigningMemo(MemoId Id) : ICommand<MemoId>;
    
    /// <summary>
    /// Отклонить памятку
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="UserId">Пользователь, отклонивший документ</param>
    /// <param name="RejectionReason">Причина отклонения</param>
    [Description("Отклонить памятку")]
    public record RejectMemo(MemoId Id, string UserId, RejectionReason RejectionReason) : ICommand<MemoId>;

    /// <summary>
    /// Подтвердить отклонение памятки
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    [Description("Подтвердить отклонение памятки")]
    public record ConfirmRejectionMemo(MemoId Id) : ICommand<MemoId>;

    /**************
    ** События **
    *************/

    /// <summary>
    /// Памятка загружена из ЭТРАН
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="Xml">Данные из ЭТРАН</param>
    public record MemoLoadedFromEtran(MemoId Id, string Xml) : IAggregateEvent<MemoId>;
    
    /// <summary>
    /// Памятка обновлена
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="Document">Документ</param>
    public record MemoUpdated(MemoId Id, MemoDocument Document) : IAggregateEvent<MemoId>;
    
    /// <summary>
    /// Запущено подписание в Этран
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    public record SigningStarted(MemoId Id, string UserId) : IAggregateEvent<MemoId>;

    /// <summary>
    /// Памятка подписана в Этран
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    public record EtranSigningConfirmed(MemoId Id) : IAggregateEvent<MemoId>;
    
    /// <summary>
    /// Памятка подписана
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    public record MemoSigned(MemoId Id) : IAggregateEvent<MemoId>;
    
    /// <summary>
    /// Запущено отклонение в Этран
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    /// <param name="UserId">Пользователь, отклонивший документ</param>
    /// <param name="RejectionReason">Причина отклонения</param>
    public record RejectionStarted(MemoId Id, string UserId, RejectionReason RejectionReason) : IAggregateEvent<MemoId>;

    /// <summary>
    /// Памятка отклонена в Этран
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    public record EtranRejectionConfirmed(MemoId Id) : IAggregateEvent<MemoId>;
    
    /// <summary>
    /// Памятка отклонена
    /// </summary>
    /// <param name="Id">Идентификатор Памятки</param>
    public record MemoRejected(MemoId Id) : IAggregateEvent<MemoId>;
    
    
    /***********************
    ** Интерфейс Агрегата **
    ***********************/
    
    /// <summary>
    /// Агрегат Памятка
    /// </summary>
    public interface IMemo : IAggregate<MemoId>,
        ICanDo<UpdateMemo, MemoId>,
        ICanDo<SignMemo, MemoId>,
        ICanDo<ConfirmSigningMemo, MemoId>,
        ICanDo<RejectMemo, MemoId>,
        ICanDo<ConfirmRejectionMemo, MemoId>
    {
        /// <summary>
        /// Обновить Памятку
        /// </summary>
        /// <param name="document">Документ</param>
        /// <returns></returns>
        public Task<CommandResult> Update(MemoDocument document) 
            => Do(new UpdateMemo(this.GetId<MemoId>(), document));

        /// <summary>
        /// Подписать памятку
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <returns></returns>
        public Task<CommandResult> Sign(string userId) 
            => Do(new SignMemo(this.GetId<MemoId>(), userId));
        
        
        /// <summary>
        /// Отклонить памятку
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="rejectionReason">Причина отклонения</param>
        /// <returns></returns>
        public Task<CommandResult> Reject(string userId, RejectionReason rejectionReason) 
            => Do(new RejectMemo(this.GetId<MemoId>(), userId, rejectionReason));

        /// <summary>
        /// Подтвердить подписание
        /// </summary>
        /// <returns></returns>
        public Task<CommandResult> ConfirmSigning() 
            => Do(new ConfirmSigningMemo(this.GetId<MemoId>()));

        /// <summary>
        /// Подтвердить отклонение
        /// </summary>
        /// <returns></returns>
        public Task<CommandResult> ConfirmRejection() 
            => Do(new ConfirmRejectionMemo(this.GetId<MemoId>()));
        
    }
}