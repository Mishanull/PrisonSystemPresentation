using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorLoginApp.Authentication;

public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService authService;

    public SimpleAuthenticationStateProvider(IAuthService authService)
    {
        this.authService = authService;
        authService.OnAuthStateChanged += AuthStateChanged;
    }

    private void AuthStateChanged(ClaimsPrincipal obj)
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult<AuthenticationState>(new AuthenticationState(obj)));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal principal = await authService.GetAuthAsync();
        return await Task.FromResult(new AuthenticationState(principal));
    }
    
}