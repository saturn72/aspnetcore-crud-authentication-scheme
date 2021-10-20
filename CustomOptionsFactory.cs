using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace aspnetcore_crud_authentication_scheme
{
    public class OpenIdConnectOptionsFactory : IOptionsFactory<OpenIdConnectOptions>
    {
        private IConfigureOptions<OpenIdConnectOptions>[] _setups;
        private readonly IPostConfigureOptions<OpenIdConnectOptions>[] _postConfigures;
        private readonly IValidateOptions<OpenIdConnectOptions>[] _validations;

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="setups">The configuration actions to run.</param>
        /// <param name="postConfigures">The initialization actions to run.</param>
        public OpenIdConnectOptionsFactory(IEnumerable<IConfigureOptions<OpenIdConnectOptions>> setups, IEnumerable<IPostConfigureOptions<OpenIdConnectOptions>> postConfigures) : this(setups, postConfigures, validations: Array.Empty<IValidateOptions<OpenIdConnectOptions>>())
        { }

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="setups">The configuration actions to run.</param>
        /// <param name="postConfigures">The initialization actions to run.</param>
        /// <param name="validations">The validations to run.</param>
        public OpenIdConnectOptionsFactory(IEnumerable<IConfigureOptions<OpenIdConnectOptions>> setups, IEnumerable<IPostConfigureOptions<OpenIdConnectOptions>> postConfigures, IEnumerable<IValidateOptions<OpenIdConnectOptions>> validations)
        {
            // The default DI container uses arrays under the covers. Take advantage of this knowledge
            // by checking for an array and enumerate over that, so we don't need to allocate an enumerator.
            // When it isn't already an array, convert it to one, but don't use System.Linq to avoid pulling Linq in to
            // small trimmed applications.

            _setups = setups as IConfigureOptions<OpenIdConnectOptions>[] ?? new List<IConfigureOptions<OpenIdConnectOptions>>(setups).ToArray();
            _postConfigures = postConfigures as IPostConfigureOptions<OpenIdConnectOptions>[] ?? new List<IPostConfigureOptions<OpenIdConnectOptions>>(postConfigures).ToArray();
            _validations = validations as IValidateOptions<OpenIdConnectOptions>[] ?? new List<IValidateOptions<OpenIdConnectOptions>>(validations).ToArray();
        }

        public void AddOption(string name, Action<OpenIdConnectOptions> action)
        {
            var e = new List<IConfigureOptions<OpenIdConnectOptions>>(_setups);
            var toAdd = new ConfigureNamedOptions<OpenIdConnectOptions>(name, action);
            e.Add(toAdd);
            _setups = e.ToArray();
        }
        /// <summary>
        /// Returns a configured <typeparamref name="OpenIdConnectOptions"/> instance with the given <paramref name="name"/>.
        /// </summary>
        public OpenIdConnectOptions Create(string name)
        {
            OpenIdConnectOptions options = CreateInstance(name);
            foreach (IConfigureOptions<OpenIdConnectOptions> setup in _setups)
            {
                if (setup is IConfigureNamedOptions<OpenIdConnectOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else if (name == Options.DefaultName)
                {
                    setup.Configure(options);
                }
            }
            foreach (IPostConfigureOptions<OpenIdConnectOptions> post in _postConfigures)
            {
                post.PostConfigure(name, options);
            }

            if (_validations.Length > 0)
            {
                var failures = new List<string>();
                foreach (IValidateOptions<OpenIdConnectOptions> validate in _validations)
                {
                    ValidateOptionsResult result = validate.Validate(name, options);
                    if (result is not null && result.Failed)
                    {
                        failures.AddRange(result.Failures);
                    }
                }
                if (failures.Count > 0)
                {
                    throw new OptionsValidationException(name, typeof(OpenIdConnectOptions), failures);
                }
            }

            return options;
        }

        /// <summary>
        /// Creates a new instance of options type
        /// </summary>
        protected virtual OpenIdConnectOptions CreateInstance(string name)
        {
            return Activator.CreateInstance<OpenIdConnectOptions>();
        }
    }
}