using static UsersManager.Application.DTOs.PermissaoDTO;

namespace UsersManager.Application.Utils
{
    public static class Extensions
    {
        public static ActionEnum ToActionEnum(this String actionEnum)
        {
            switch (actionEnum.ToLower())
            {
                case "update":
                    return ActionEnum.Update;
                case "delete":
                    return ActionEnum.Delete;
                case "read":
                    return ActionEnum.Read;
                case "create":
                    return ActionEnum.Create;
                default:
                    return ActionEnum.Read;

            }
        }
    }
}
