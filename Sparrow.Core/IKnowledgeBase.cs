namespace Sparrow
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IKnowledgeBase<TMask>
    {
        Task<uint> AddMaskedFileNameAsync(MaskedFileName<TMask> maskedFileName);

        Task AddTokenizerForMaskedFileNameAsync(uint maskedFileNameId, FileNameTokenizer tokenizer);

        Task<IList<MaskedFileName<TMask>>> GetMaskFileNamesByTokenizerAsync(FileNameTokenizer tokenizer);
    }
}
