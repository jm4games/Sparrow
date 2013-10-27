using System.Collections.Generic;

namespace Sparrow.Core
{
    public interface IKnowledgeBase
    {
        MaskedFileNameEntity CreateMaskedFileNameEntity(IMaskedFileName maskedFileName);

        void AddTokenizedFileNameForMaskedFileName(uint maskedFileNameId, FileNameTokenizer tokenizer);

        IList<MaskedFileNameEntity> GetMaskFileNameEntities(FileNameTokenizer tokenizer);
    }
}
