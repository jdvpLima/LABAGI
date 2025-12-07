using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Workshop
{
    public class LocalContentFilter : MonoBehaviour
    {
        [Header("Profanity List (EN)")]
        [SerializeField] private TextAsset englishProfanityFile;

        [Header("Wordlists")]
        [SerializeField] private TextAsset englishWordsFile;
        [SerializeField] private int minWordLength = 3;

        private HashSet<string> _bannedTermsNormalized;
        private HashSet<string> _englishWords;

        // só letras A-Z (maiúsculas/minúsculas), ignora números/pontuação
        private static readonly Regex WordRegex = new Regex(@"[A-Za-z0-9][A-Za-z0-9'-]*", RegexOptions.Compiled);

        private void Awake()
        {
            LoadProfanityList();
            LoadWordlists();
        }
        private void LoadWordlists()
        {
            _englishWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (englishWordsFile != null && !string.IsNullOrWhiteSpace(englishWordsFile.text))
            {
                var lines = englishWordsFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in lines)
                {
                    var w = raw.Trim();
                    if (!string.IsNullOrEmpty(w))
                        _englishWords.Add(w);
                }
            }
            else
            {
                Debug.LogError("[LocalContentFilter] englishWordsFile empty.");
            }
        }

        private void LoadProfanityList()
        {
            _bannedTermsNormalized = new HashSet<string>();

            if (englishProfanityFile == null || string.IsNullOrWhiteSpace(englishProfanityFile.text))
                return;

            var lines = englishProfanityFile.text
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"));

            foreach (var line in lines)
            {
                var norm = Normalize(line);
                if (!string.IsNullOrEmpty(norm))
                    _bannedTermsNormalized.Add(norm);
            }
        }

        public bool ContainsOffensiveContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || _bannedTermsNormalized == null)
                return false;

            var normalized = Normalize(text);

            foreach (var term in _bannedTermsNormalized)
            {
                if (normalized.Contains(term))
                    return true;
            }

            return false;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.ToLowerInvariant();

            // remover acentos, espaços, pontuação, etc., se quiseres
            var chars = input.Where(char.IsLetterOrDigit);
            return new string(chars.ToArray());
        }

        public bool IsStrictEnglish(string text, out string unknownWord)
        {
            unknownWord = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (_englishWords == null || _englishWords.Count == 0)
            {
                Debug.LogWarning("[LocalContentFilter] _englishWords not loaded.");
                return true; // ou false, se quiseres ser super estrito
            }

            var matches = WordRegex.Matches(text);

            foreach (Match m in matches)
            {
                var token = m.Value;// ex: "48-point", "Hello", "cão"

                // dividir por hífen para tratar “48-point” e “X-ray”
                var parts = token.Split('-');

                foreach (var rawPart in parts)
                {
                    var part = rawPart.Trim();
                    if (string.IsNullOrEmpty(part))
                        continue;

                    // ignorar partes que sejam só números (ex: "48")
                    if (part.All(char.IsDigit))
                        continue;

                    if (part.Length < minWordLength)
                        continue;

                    var key = part.ToLowerInvariant();

                    // se não está no dicionário, não é inglês
                    if (!_englishWords.Contains(key))
                    {
                        unknownWord = part;
                        return false;
                    }
                }
            }

            return true;
        }

    }
}

