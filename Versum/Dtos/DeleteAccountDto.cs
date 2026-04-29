namespace Versum.Dtos
{
    public class DeleteAccountDto
    {
        public string Password { get; set; } = string.Empty;
        public string ConfirmWord {  get; set; }  = string.Empty;

    }
}
