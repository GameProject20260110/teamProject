using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class DeckSaveManager
{
    private const string DeckFolderName = "decks";
    private const string IndexFileName = "deck_index.json";

    private readonly SaveManager _saveManager;
    private readonly string deckFoldersPath;
    private readonly string indexRelativePath; // SaveManager에 넘기는 상대 경로

    private DeckIndex _cachedIndex;

    public DeckSaveManager(SaveManager saveManager, int maxDeckSlots = 5)
    {
        _saveManager = saveManager;
        deckFoldersPath = Path.Combine(Application.persistentDataPath, DeckFolderName);
        indexRelativePath = Path.Combine(DeckFolderName, IndexFileName);

        try
        {
            if (!Directory.Exists(deckFoldersPath))
                Directory.CreateDirectory(deckFoldersPath);
        }
        catch(Exception e)
        {
            Debug.LogError($"[DeckSaveManager] 폴더 생성 실패 : {e.Message}");
        }

        _cachedIndex = LoadIndex();
        _cachedIndex.maxDeckSlots = maxDeckSlots;
        ReconcileIndexWithDeckFiles();
    }

    public DeckIndex index => _cachedIndex;

    // 덱 시스템을 처음 쓰는 지 확인
    public static bool HasExistingIndex(SaveManager saveManager) => saveManager != null && saveManager.HasSaveFile(Path.Combine(DeckFolderName, IndexFileName));

    public bool CanCreateNewDeck() => _cachedIndex.entries.Count < _cachedIndex.maxDeckSlots;

    public bool SaveDeck(DeckData deck, out string errorMessage)
    {
        if(deck == null)
        {
            errorMessage = "저장에 실패하였습니다.";
            return false;
        }

        bool isExisting = _cachedIndex.entries.Exists(e => e.deckId == deck.deckId);
        if(!isExisting && !CanCreateNewDeck())
        {
            Debug.LogWarning("[DeckSaveManager] 생성할 수 있는 덱 슬롯을 전부 사용했습니다. 저장 할 수 없습니다.");
            errorMessage = $"저장 가능한 덱 갯수({_cachedIndex.maxDeckSlots})개를 초과했습니다.";
            return false;
        }

        deck.lastModifiedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if(!_saveManager.Save(deck, GetDeckRelativePath(deck.deckId)))
        {
            errorMessage = "덱 저장 중 오류가 발생했습니다.";
            return false;
        }

        UpsertIndexEntry(deck);

        if(!SaveIndex())
        {
            errorMessage = "덱은 저장됐지만 목록 갱신에 실패했습니다.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public DeckData LoadDeck(string deckId)
    {
        if(!_saveManager.Load(GetDeckRelativePath(deckId), out DeckData data))
        {
            Debug.LogWarning($"[DeckSaveManager] 덱을 불러오지 못했습니다.: {deckId}");
            return null;
        }
        return data;
    }

    public List<string> DeleteDecks(IEnumerable<string> deckIds)
    {
        List<string> failedDeckIds = new List<string>();
        if(deckIds == null) return failedDeckIds;

        bool changed = false;

        foreach(string deckId in deckIds)
        {
            if(!_saveManager.Delete(GetDeckRelativePath(deckId)))
            {
                failedDeckIds.Add(deckId);
                continue;
            }
            _cachedIndex.entries.RemoveAll(entry => entry.deckId == deckId);
            changed = true;
        }
        if (changed) SaveIndex();
        return failedDeckIds;
    }

    public string CreateNewDeckId() => Guid.NewGuid().ToString("N");

    public bool SetActiveDeck(string deckId)
    {
        if (!_cachedIndex.entries.Exists(e => e.deckId == deckId)) return false;
        _cachedIndex.activeDeckId = deckId;
        return SaveIndex();
    }

    private void UpsertIndexEntry(DeckData deck)
    {
        DeckIndexEntry entry = _cachedIndex.entries.Find(e => e.deckId == deck.deckId);
        if (entry == null)
        {
            entry = new DeckIndexEntry { deckId = deck.deckId };
            _cachedIndex.entries.Add(entry);
        }
        entry.deckName = deck.deckName;
        entry.lastModifiedUnix = deck.lastModifiedUnix;
        entry.coverCardId = deck.cardIds != null && deck.cardIds.Count > 0 ? deck.cardIds[0] : -1;
    }

    private DeckIndex LoadIndex()
    {
        _saveManager.Load(indexRelativePath, out DeckIndex loaded);
        return loaded;
        
    }

    private bool SaveIndex() => _saveManager.Save(_cachedIndex, indexRelativePath);

    private void ReconcileIndexWithDeckFiles()
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(deckFoldersPath, "*.json");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DeckSaveManager] 인덱스 폴더를 동기화 하는 과정에서 스캔에 실패했습니다 : {e.Message}");
            return;
        }

        bool changed = false;
        string indexFileNameOnly = Path.GetFileNameWithoutExtension(IndexFileName);

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName == indexFileNameOnly) continue;

            bool alreadyIndexed = _cachedIndex.entries.Exists(e => e.deckId == fileName);
            if (alreadyIndexed) continue;

            if(!_saveManager.Load(GetDeckRelativePath(fileName), out DeckData recovered) || string.IsNullOrEmpty(recovered.deckId))
            {
                Debug.LogError($"[DeckSaveManager] 덱 파일을 해석하는데 실패했습니다. : {fileName}");
                continue;
            }

            if(recovered.deckId != fileName)
            {
                Debug.LogWarning($"[DeckSaveManager] 파일 이름({fileName})과 덱 아이디({recovered.deckId})가 일치하지 않습니다.");
                 recovered.deckId = fileName;
                _saveManager.Save(recovered, GetDeckRelativePath(fileName));
            }

            _cachedIndex.entries.Add(new DeckIndexEntry
            {
                deckId = recovered.deckId,
                deckName = recovered.deckName,
                lastModifiedUnix = recovered.lastModifiedUnix,
                coverCardId = recovered.cardIds != null && recovered.cardIds.Count > 0 ? recovered.cardIds[0] : -1
            });
            changed = true;
            Debug.LogWarning($"[DeckSaveManager] 인덱스에서 누락된 덱 파일을 발견하여 복구했습니다. : {recovered.deckId}");
       
        }
        if (changed) SaveIndex();
    }

    private string GetDeckRelativePath(string deckId) => Path.Combine(DeckFolderName, $"{deckId}.json");
}
