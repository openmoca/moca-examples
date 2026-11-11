# MOCA examples

Small examples of reading MOCA packages, in .NET and TypeScript side by side.
Each language has the same two:

| Example | What it shows |
| --- | --- |
| **CLI** | Open a package and see its digest, capabilities and diagnostics; search it with citations; get one node's citation record. |
| **Chat** | A basic chat over a package: search it, hand the hits to a model as cited data, and print the answer with its sources. Any OpenAI-compatible endpoint: [Ollama](https://ollama.com) on your machine by default, or your own key. |

The TypeScript examples use the reference Reader,
[`@openmoca/moca-core`](https://github.com/openmoca/moca-reader-typescript).
The .NET examples use [`OpenMoca.Reader`](https://github.com/openmoca/moca-reader-dotnet).
The example packages are in
[moca-spec](https://github.com/openmoca/moca-spec/tree/main/examples).

## Set up

The Readers come from npm and NuGet. The examples read the example packages
from a moca-spec checkout beside this repository:

```text
moca-spec/
moca-examples/
```

The tests find moca-spec there, or in `MOCA_SPEC_DIR`. CI uses the tags pinned at the top of `.github/workflows/test.yml`
(`MOCA_SPEC_REF` and the like); check out the same tags to match it.

## Run the examples

The same commands in each language: open a package, search it, get one
node's citation record, then chat over it.

<!-- tabs -->

**TypeScript:**

Node.js 22 or later.

```sh
cd typescript && npm install

node cli.js open   ../../moca-spec/examples/support-kb
node cli.js search ../../moca-spec/examples/support-kb refund window
node cli.js get    ../../moca-spec/examples/support-kb "https://example.com/moca/support-kb@4.2.0#refund-window.md"

node chat.js ../../moca-spec/examples/support-kb How long is the refund window
npm test
```

**.NET:**

The .NET 10 SDK.

```sh
cd dotnet

dotnet run --project Moca.Examples.Cli -- open   ../../moca-spec/examples/support-kb
dotnet run --project Moca.Examples.Cli -- search ../../moca-spec/examples/support-kb refund window
dotnet run --project Moca.Examples.Cli -- get    ../../moca-spec/examples/support-kb "https://example.com/moca/support-kb@4.2.0#refund-window.md"

dotnet run --project Moca.Examples.Chat -- ../../moca-spec/examples/support-kb How long is the refund window
dotnet test
```

<!-- /tabs -->

## The chat's model

The chat calls any OpenAI-compatible chat endpoint:

| Variable | Default |
| --- | --- |
| `MOCA_CHAT_BASE_URL` | `http://localhost:11434/v1` (Ollama) |
| `MOCA_CHAT_MODEL` | `qwen3:0.6b` (`ollama pull qwen3:0.6b`) |
| `OPENAI_API_KEY` | None. Set it to use a hosted provider; it is sent as a bearer token. |

Add `--show-prompt` to print the exact messages the chat would send, without
calling a model.

**Package text is untrusted input.** The chat sends it to the model as
delimited, numbered sources in the user's turn, and tells the model they are
reference material to quote and cite, never instructions
([Reader contract §12](https://github.com/openmoca/moca-spec/blob/main/spec/moca-reader-contract.md#12-handing-content-to-a-model)).
Do the same in your own code.

## Licence

Apache-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).

OpenMOCA™ is a trade mark of PI & Mash Solutions Ltd. The licence covers the
code, not the name: see the
[trade mark policy](https://github.com/openmoca/moca-spec/blob/main/TRADEMARKS.md).
