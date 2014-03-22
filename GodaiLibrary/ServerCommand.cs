using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    public enum EServerCommand
    {
        AddUser = 1,
        TryLogon,
        GetDungeon,
        SetDungeon,
        GetDungeonBlockImage,
        SetDungeonBlockImage,
        GetTilePalette,
        SetTilePalette,
        GetObjectAttrInfo,
        SetObjectAttrinfo,
        GetTileList,
        GetUserInfo,
        SetAItem,
        GetItemInfo,
        GetAItem,
        SetAUser,
        ChangeAItem,
        UploadItemFiles,
        Polling,
        GetIslandGroundInfo,
        ChangeObjectAttr,
        ChangeDungeonBlockImagePair,
        GetLocationInfo,
        GetMessageInfo,
        SetAMessage,
        GetExpValue,
        GetUnpickedupItemInfo,
        GetAshiato,
        GetAshiatoLog,
        GetArticleString,
        SetItemArticle,
        ReadArticle,
        DeleteLastItemArticle,
        UseExperience,
        GetDungeonDepth,
        GetMonsterInfo,
        SetMonster,
        GetRDReadItemInfo,
        SetRDReadItem,
        ChangePassword,
        SetUserFolder,
        GetUserFolder,
		GetRealMonsterSrcInfo, // モンスターの元情報を得る
		GetItemInfoByUserId,
        RegisterKeyword,
        ModifyKeyword,
        ModifyKeywordPriority,
        AttachKeyword,
        DetachKeyword,
        ListKeyword,
        GetKeywordDetail,
        ModifyKeywordItemPriority,
        DeleteKeyword
    }

    public enum EServerResult
    {
        UnknownError = 0,
        SUCCESS = 1,
        UserAlreadyExist = 2,           // すでに同じメールアドレスのユーザが存在した
        PasswordWrong,                  // パスワードが間違っています
        MissingUser,                    // ユーザが存在しません
        MissingDungeon,                 // ダンジョンが存在しません
        RequireLogon,                   // ログインする必要があります
        ClientObsolete,                 // クライアントが古過ぎます
        MissingItem,                    // アイテムが見つかりません
        AlreadyLogin,                   // すでにログイン済みです
        MissingObject,                  // オブジェクトが見つかりません
        MissingDugeonBlockImage,        // イメージが見つかりません
        AlreadyRead,                    // 読み込み済みなのである
        NotYourArticle,                 // アーティクルがあなたのものではありません
        NotEnoughExp,                   // 経験値が足りません
        NotExistDungeon,                // ダンジョンが存在しません
        ClientNewer,                    // クライアントが新しすぎます
		SameKeyword,					// 同じキーワードがすでにあります
		MissingKeyword					// キーワードが存在しない
    }

    public enum EUseExp
    {
        ExpandX,
        ExpandY,
        CreateNewFloor
    }
}
