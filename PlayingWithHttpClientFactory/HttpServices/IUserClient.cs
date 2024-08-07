﻿namespace PlayingWithHttpClientFactory.HttpServices;

public interface IUserClient
{
    /// <summary>
    /// Get the users.
    /// </summary>
    /// <exception cref="ServiceException">Thrown when something went wrong.</exception>
    /// <returns>Some kind of users.</returns>
    Task<IEnumerable<string>> GetUsersAsync(CancellationToken ct = default);
}
