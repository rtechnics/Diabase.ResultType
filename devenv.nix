{
  config,
  lib,
  pkgs,
  ...
}:
{
  options = {
    languages.dotnet.packageSuffix = lib.mkOption {
      type = lib.types.str;
      default =
        let
          version = (builtins.fromJSON (builtins.readFile ./global.json)).sdk.version;
        in
        "${lib.versions.major version}_${lib.versions.minor version}";
    };
  };
  config = {
    name = "diabase-resulttype";
    languages = {
      dotnet = {
        enable = true;
        package = pkgs.dotnetCorePackages."sdk_${config.languages.dotnet.packageSuffix}";
        lsp.package = pkgs.omnisharp-roslyn;
      };
      nix.enable = true;
      shell.enable = true;
    };
    scripts = {
      actions-build.exec = "${lib.getExe pkgs.nix} run .#actions-container-shell.copyToPodman --impure --accept-flake-config";
      actions-bash.exec = "${lib.getExe pkgs.podman-compose} -f compose.actions_linux.yaml run --rm shell -- bash $@";
      act.exec = "${lib.getExe pkgs.podman-compose} -f compose.actions_linux.yaml run --rm shell -- act $@";
    };
    packages = [
      pkgs.docker-compose-language-service
      pkgs.podman-compose
      pkgs.vscode-json-languageserver
      pkgs.yaml-language-server
    ];
    treefmt = {
      enable = true;
      config.programs = {
        # formatjson5.enable = true; # Disabled due to machine-generated files
        nixfmt = {
          enable = true;
          strict = true;
        };
        # pinact.enable = true; # Should automate this
        shfmt.enable = true;
        yamlfmt = {
          enable = true;
          settings = {
            formatter = {
              type = "basic";
              include_document_start = true; # yamllint fails without this
              pad_line_comments = 2; # Required by yamllint
            };
          };
        };
      };
    };
    git-hooks.hooks = {
      actionlint.enable = true;
      # check-json.enable = true; # Disabled due to machine-generated files
      flake-checker.enable = true;
      shellcheck.enable = true;
      yamllint = {
        enable = true;
        # actionslint does not support explicit YAML version declarations
        settings.configuration = ''
          ---
          extends: default
          rules:
            truthy:
              check-keys: false
        '';
      };
    };
  };
}
