# MPF.ExecutionContexts

This library represents the logic needed to invoke 3 different dumping programs:

- [Aaru](github.com/aaru-dps/Aaru)
- [DiscImageCreator](github.com/saramibreak/DiscImageCreator)
- [Redumper](https://github.com/superg/redumper)

These execution wrappers allow for generating valid parameters for each of the programs as well as provide some helpers that make it easier to determine what those parameters are.

External options are defined in order to help create reasonable default parameter sets for different combinations of media type and system.

Paths to the programs need to be provided if they are expected to be invoked as a part of another program. The expected versions of these programs can be found in either the changelog or the publish script in the parent project.
