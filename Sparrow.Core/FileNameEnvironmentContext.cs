namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public sealed class FileNameEnvironmentContext<TMask, TKnow> : IEquatable<FileNameEnvironmentContext<TMask, TKnow>> where TKnow : IKnowledgeBase
    {
        private readonly string filePath;

        public FileNameEnvironmentContext(string filePath, IDictionary<TMask, MaskConfiguration> masks, TKnow knowledgeBase)
        {
            if (!filePath.IsValidFilePath())
            {
                throw new ArgumentException("The provided file path '" + filePath ?? String.Empty + "' is not valid.", "filePath");
            }

            if (masks == null)
            {
                throw new ArgumentNullException("masks");
            }

            if (knowledgeBase == null)
            {
                throw new ArgumentNullException("knowledgeBase");
            }

            this.filePath = filePath;
            this.KnowledgeBase = knowledgeBase;
            this.MaskedFileName = new MaskedFileName<TMask>(new FileNameTokenizer(Path.GetFileNameWithoutExtension(filePath)), masks);
            this.SourceDirectory = Path.GetDirectoryName(filePath);
            this.FileNameProcessedCompletionSource = new TaskCompletionSource<string>();
            this.MasksResolvedCount = 0;
        }

        public MaskedFileName<TMask> MaskedFileName { get; private set; }
                
        public string SourceDirectory { get; private set; }

        public TKnow KnowledgeBase { get; private set; }

        public bool AllTokensHaveMask { get { return this.MasksResolvedCount == MaskedFileName.Tokenizer.TokenCount; } }

        internal int MasksResolvedCount { get; set; }

        internal TaskCompletionSource<string> FileNameProcessedCompletionSource { get; private set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FileNameEnvironmentContext<TMask, TKnow>);
        }

        public bool Equals(FileNameEnvironmentContext<TMask, TKnow> other)
        {
            if (other == null)
            {
                return false;
            }

            return other != null && other.filePath == this.filePath;
        }

        public override int GetHashCode()
        {
            return this.filePath.GetHashCode();
        }
    }
}
