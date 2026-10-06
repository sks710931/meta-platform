# Organizations

CreateOrganization, ListOrganizations, and GetOrganizationDetails are the implemented slices. They use only Domain and the context-specific IOrganizationStore port. Responses are explicit application contracts; persistence and HTTP types do not enter the slices.
