# Actions artifact retention

Keep the current completed build and two previous completed builds per artifact family (three build sets total). All files in a retained build set stay together; versioned installer names share one family. Docker build records are grouped by their publishing workflow.

Cleanup runs after `Build` or `Build Installer` completes, daily, and when this policy is merged. It also handles historical artifacts whose original expiry dates have not changed. Failed builds with diagnostics count as completed builds.

Active, queued, waiting, unavailable, expired, and unrecognized runs/artifacts are not deleted. Run status is checked again immediately before deleting a build's artifacts. Temporary extra build sets can remain while runs are active. A manual run defaults to preview mode; automatic cleanup performs deletion.

The cleanup job uses only default-branch code, does not download or execute artifacts, and has Actions write permission only in its cleanup job. Pull requests run the retention tests without deletion permission. API/permission failures are reported instead of silently disabling cleanup.

Existing artifact expiry ceilings still apply: this count policy is a maximum, not a promise to keep files past their expiry. The existing per-artifact size caps are unchanged; no arbitrary new size cap is introduced. Retaining three builds cannot by itself guarantee that the account-wide storage quota is sufficient.

This policy covers these artifact names:
- `^MdReaderSetup-` → installer
- `^installer-diagnostics$` → installer-diagnostics
- `^MdReader-win-x64$` → application

GitHub Release assets, container images, dependency/build caches, and application business-data history are not deleted by this workflow.

Run the focused regression checks with:

```powershell
node --test .github/retention/prune-artifacts.test.cjs
```
